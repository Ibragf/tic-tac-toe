using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Web;
using tic_tac_toe.Db;
using tic_tac_toe.Models;
using tic_tac_toe.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace tic_tac_toe.Controllers
{
    [Route("api/lobbies")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    public class LobbyController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        private readonly IDateProvider dateProvider;
        private readonly IHubContext<GameHub> hubContext;
        public LobbyController(AppDbContext dbContext, IDateProvider dateProvider, IHubContext<GameHub> hubContext)
        {
            this.dbContext = dbContext;
            this.dateProvider = dateProvider;
            this.hubContext = hubContext;
        }


        [HttpGet]
        public async Task<IActionResult> CreateLobby([Required] string playerId, [Required] string connectionId)
        {
            var player = await dbContext.Players.FirstOrDefaultAsync(x => x.Id == playerId);

            if(player == null) return NotFound();
            if (player.ConnectionId == null || player.ConnectionId != connectionId)
                return Problem(detail: "Connection Id is invalid");

            var lobby = new Lobby
            {
                Id = Guid.NewGuid().ToString(),
                CreatedDate = dateProvider.GetCurrentDate(),
                OwnerId = player.Id,
                Players = new List<Player>(),
                Scores = new List<Score>(),
            };

            var score = new Score { Id = Guid.NewGuid().ToString(), LobbyId = lobby.Id, PlayerId = playerId };

            lobby.Players.Add(player);
            lobby.Scores.Add(score);
            dbContext.Lobbies.Add(lobby);

            await dbContext.SaveChangesAsync();

            return Ok(lobby.Id);
        }

        [HttpPut]
        public async Task<IActionResult> JoinLobby(LobbyInput input)
        {
            var player = await dbContext.Players.FirstOrDefaultAsync(x => x.Id == input.playerId);

            if (player == null)
            {
                var problemDetails = new ProblemDetails { Detail = "The player not found" };
                return NotFound(problemDetails);
            }

            if (player.ConnectionId == null || player.ConnectionId != input.connectionId)
                return Problem(detail: "Connection Id is invalid");

            var lobby = await dbContext.Lobbies.Include(x => x.Players).FirstOrDefaultAsync(x => x.Id == input.lobbyId && x.OwnerId != player.Id);

            if (lobby == null)
            {
                var problemDetails = new ProblemDetails { Detail = "The lobby not found" };
                return NotFound(problemDetails);
            };

            if(lobby.Players.Count >= 2)
                return Problem(detail: "The lobby is already full");

            if(lobby.Closed)
                return Problem(detail: "The lobby is closed");

            var score = new Score { Id = Guid.NewGuid().ToString(), PlayerId = player.Id, LobbyId = lobby.Id };

            lobby.Players.Add(player);
            dbContext.Scores.Add(score);
            await dbContext.SaveChangesAsync();

            var secondPlayer = await dbContext.Players.FirstOrDefaultAsync(x => x.Id != player.Id);
            if(secondPlayer == null)
            {
                var problemDetails = new ProblemDetails { Detail = "The second player not found" };
                return NotFound(problemDetails);
            }

            string encodedName = HttpUtility.HtmlEncode(player.Name);
            await hubContext.Clients.Client(secondPlayer.ConnectionId).SendAsync("playerJoined", encodedName);

            return Ok();
        }
    }

    public class LobbyInput
    {
        [Required]
        public string playerId { get; set; }
        [Required]
        public string lobbyId { get; set; }
        [Required]
        public string connectionId { get; set; }
    }
}
