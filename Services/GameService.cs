using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using tic_tac_toe.Db;
using tic_tac_toe.Models;

namespace tic_tac_toe.Services
{
    public class GameService
    {
        private readonly AppDbContext dbContext;
        private readonly IDateProvider dateProvider;
        public Lobby Lobby { get; private set; }
        public GameService(AppDbContext dbContext, IDateProvider dateProvider)
        {
            this.dbContext = dbContext;
            this.dateProvider = dateProvider;   
        }

        public async Task<MoveResult> MakeMoveAsync(Move move)
        {
            if(move == null) throw new ArgumentNullException(nameof(move));
            MoveResult moveResult = null;

            var game = await ValidateMoveModelAsync(move, moveResult!);
            if (moveResult != null) return moveResult;

            move.CreatedDate = dateProvider.GetCurrentDate();
            game.Moves.Add(move);

            var winMoves = isGameWon(game.Moves.Where(x => x.PlayerId == move.PlayerId).ToList());
            if (winMoves != null)
            {
                var score = await dbContext.Scores.FirstOrDefaultAsync(x => x.LobbyId == Lobby.Id && x.PlayerId == move.PlayerId);
                score.Victories += 1;

                game.Winner = move.PlayerId;
                game.EndTime = dateProvider.GetCurrentDate();
                return new MoveResult { Success = true, Move = move, WinningsMove = winMoves };
            }
            if(game.Moves.Count == 9)
            {
                game.Winner = "Draw";
                game.EndTime = dateProvider.GetCurrentDate();
                return new MoveResult { Success = true, Move = move, isDraw = true };
            }

            return new MoveResult { Success = true, Move = move };
        }
        private async Task<Game> ValidateMoveModelAsync(Move move, MoveResult moveResult)
        {
            Lobby = await dbContext.Lobbies.Include(x => x.Games).FirstOrDefaultAsync(x => x.Players.Any(p => p.Id == move.PlayerId));
            if (Lobby == null)
            {
                moveResult = new MoveResult() { Success = false, Move = move, Errors = new List<string> { "The lobby not found" } };
                return null!;
            }

            var game = Lobby.Games.OrderByDescending(x => x.CreatedDate).FirstOrDefault();

            if(game == null) 
            {
                moveResult = new MoveResult() { Success = false, Move = move, Errors = new List<string> { "The game not found" } };
                return game!;
            }

            if (game.MoveOwnerId != move.PlayerId)
                moveResult = new MoveResult() { Success = false, Move = move, Errors = new List<string> { "The move is owned by another player." } };

            if(game.Winner != null)
                moveResult = new MoveResult() { Success = false, Move = move, Errors = new List<string> { "The game is over" } };

            return game;
        }
        private List<Move> isGameWon(List<Move> moves)
        {
            var x = new int[3];
            var y = new int[3];

            List<Move> mainDiagonal = new List<Move>();
            List<Move> crossDiagonal = new List<Move>();

            List<Move> result = null!;

            foreach (var move in moves)
            {
                x[move.X] += 1;
                y[move.Y] += 1;

                if(Math.Abs(move.X - move.Y) == 2 || (move.X==1 && move.Y==1))
                    crossDiagonal.Add(move);

                if(move.X == move.Y) mainDiagonal.Add(move);
            }

            if (mainDiagonal.Count == 3) return result = mainDiagonal;
            if (crossDiagonal.Count == 3) return result = crossDiagonal;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i]==3)
                {
                    result = new List<Move>();
                    for (int j = 0; j < x.Length; j++)
                    {
                        var winMove = new Move { X = i, Y = j};
                        result.Add(winMove);
                    }
                    return result;
                }
                if (y[i]==3)
                {
                    result = new List<Move>();
                    for (int j = 0; j < x.Length; j++)
                    {
                        var winMove = new Move { X = j, Y = i };
                        result.Add(winMove);
                    }
                    return result;
                }
            }

            return result;
        }
    }
}
