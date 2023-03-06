using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Web;
using tic_tac_toe.Db;
using tic_tac_toe.Models;

namespace tic_tac_toe.Services
{
    public class GameHub : Hub
    {
        private readonly AppDbContext dbContext;
        private readonly IDateProvider dateProvider;
        public GameHub(AppDbContext dbContext, IDateProvider dateProvider)
        {
            this.dbContext = dbContext;
            this.dateProvider = dateProvider;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var player = await dbContext.Players.FirstOrDefaultAsync(x => x.ConnectionId == Context.ConnectionId);
            if (player != null)
            {
                var lobby = await dbContext.Lobbies.FirstOrDefaultAsync(x => x.Players.Any(x => x.ConnectionId == player.ConnectionId));
                if(lobby != null)
                {
                    var secondPlayer = lobby.Players.FirstOrDefault(x => x.ConnectionId != player.ConnectionId);
                    if(secondPlayer != null)
                    {
                        lobby.Closed = true;
                        player.ConnectionId = null!;
                        Score score = null!;
                        
                        var game = lobby.Games.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                        if(game != null || game!.Winner == null)
                        {
                            game.Winner = secondPlayer.Id;
                            game.EndTime = dateProvider.GetCurrentDate();

                            score = lobby.Scores.FirstOrDefault(x => x.PlayerId == secondPlayer.Id);
                            score.Victories+=1;
                        }

                        string encoded = HttpUtility.HtmlEncode(player.Name);
                        var model = new { message = $"{encoded} disconnected", lobby.Scores };

                        await dbContext.SaveChangesAsync();

                        await Clients.Client(connectionId: secondPlayer.ConnectionId).SendAsync("playerDisconnected", model);
                    }
                    
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
