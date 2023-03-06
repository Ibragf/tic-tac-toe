using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Web;
using tic_tac_toe.Db;
using tic_tac_toe.Models;
using tic_tac_toe.Services;

namespace tic_tac_toe.Controllers
{
    [Route("api/game")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    public class GameController : ControllerBase
    {
        private readonly GameService gameService;
        private readonly IHubContext<GameHub> hubContext;
        private readonly AppDbContext dbContext;
        private readonly IDateProvider dateProvider;
        public GameController(GameService gameService, IHubContext<GameHub> hubContext, AppDbContext dbContext, IDateProvider dateProvider)
        {
            this.hubContext = hubContext;
            this.gameService = gameService;
            this.dbContext = dbContext;
            this.dateProvider = dateProvider;
        }

        [HttpPost("move")]
        public async Task<IActionResult> MakeMoveAsync(Move move)
        {
            var moveResult = await gameService.MakeMoveAsync(move);
            
            if(moveResult.Success)
            {
                var secondPlayer = gameService.Lobby.Players.FirstOrDefault(x => x.Id != move.PlayerId);
                await hubContext.Clients.Client(secondPlayer!.ConnectionId).SendAsync("MadeMove", moveResult);
                return Ok(moveResult);
            }
            else
            {
                return BadRequest(moveResult);
            }
        }

        [HttpPut("start")]
        public async Task<IActionResult> CreateGame(LobbyInput input)
        {
            var player = await dbContext.Players.FirstOrDefaultAsync(x => x.Id == input.playerId);

            if (player == null)
            {
                var problemDetails = new ProblemDetails { Detail = "The player not found" };
                return NotFound(problemDetails);
            }

            if (player.ConnectionId == null || player.ConnectionId != input.connectionId)
                return Problem(detail: "Connection Id is invalid");

            var lobby = await dbContext.Lobbies.Include(x => x.Players).Include(x => x.Games).FirstOrDefaultAsync(x => x.Id == input.lobbyId && x.OwnerId == input.playerId);
            if (lobby == null)
            {
                var problemDetails = new ProblemDetails { Detail = "The lobby not found" };
                return NotFound(problemDetails);
            }
            if (lobby.Players.Count < 2)
                return Problem(detail: "Not enough people to start the game");
            if (lobby.Closed)
                return Problem(detail: "The lobby is closed");

            var createdGame = lobby.Games.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            if (createdGame != null && createdGame.Winner == null)
            {
                return Problem(detail: "The current game is not over");
            }

            var random = new Random();
            var game = new Game()
            {
                Id = Guid.NewGuid().ToString(),
                CreatedDate = dateProvider.GetCurrentDate(),
                MoveOwnerId = lobby.Players[random.Next(0, 1)].Id,
            };

            lobby.Games.Add(game);
            await dbContext.SaveChangesAsync();

            var model = new { game, lobby.Scores };
            await hubContext.Clients.Clients(lobby.Players[0].ConnectionId, lobby.Players[1].ConnectionId).SendAsync("receiveGame", model, "Game created");

            return Ok(game.Id);
        }

        [HttpPut("restart")]
        public async Task<IActionResult> RestartGame([Required] string lobbyId, [Required] string playerId)
        {
            var lobby = await dbContext.Lobbies.Include(x => x.Players).FirstOrDefaultAsync(x => x.Id == lobbyId);
            if (lobby == null)
            {
                var problemDetails = new ProblemDetails { Detail = "The lobby not found" };
                return NotFound(problemDetails);
            }
            if (lobby.Players.Count < 2)
                return Problem(detail: "Not enough people to start the game");
            if (lobby.Closed)
                return Problem(detail: "The lobby is closed");

            var player = await dbContext.Players.FirstOrDefaultAsync(x => x.Id == playerId);
            if (player == null)
            {
                var problemDetails = new ProblemDetails { Detail = "The player not found" };
                return NotFound(problemDetails);
            }

            var score = await dbContext.Scores.FirstOrDefaultAsync(x => x.LobbyId == lobby.Id && x.PlayerId != player.Id);
            score.Victories += 1;

            var random = new Random();
            var game = new Game()
            {
                Id = Guid.NewGuid().ToString(),
                CreatedDate = dateProvider.GetCurrentDate(),
                MoveOwnerId = lobby.Players[random.Next(0, 1)].Id,
            };

            var model = new { game, lobby.Scores };
            string encoded = HttpUtility.HtmlEncode(player.Name);

            await hubContext.Clients.Clients(lobby.Players[0].ConnectionId, lobby.Players[1].ConnectionId).SendAsync("receiveGame", model, $"{encoded} restarted the game");

            return Ok();
        }
    }
}
