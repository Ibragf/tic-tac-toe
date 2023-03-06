using System.ComponentModel.DataAnnotations;

namespace tic_tac_toe.Models
{
    public class Score
    {
        [Key]
        public string Id { get; set; }
        public int Victories { get; set; }
        public string LobbyId { get; set; }
        public string PlayerId { get; set; }
    }
}
