using System.ComponentModel.DataAnnotations;

namespace tic_tac_toe.Models
{
    public class MoveResult
    {
        public bool Success { get; set; }
        public Move Move { get; set; }
        public List<Move> WinningsMove { get; set; }
        public List<string> Errors { get; set; }
        public bool isDraw { get; set; }
    }
}
