using System.ComponentModel.DataAnnotations;

namespace tic_tac_toe.Models
{
    public class Lobby
    {
        [Key]
        public string Id { get; set; }
        public bool Closed { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OwnerId { get; set; }
        public List<Score> Scores { get; set; }
        public List<Game> Games { get; set; }
        public List<Player> Players { get; set; }
    }
}
