using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace tic_tac_toe.Models
{
    public class Game
    {
        [Key]
        public string Id { get; set; }
        public string? Winner { get; set; }
        public string MoveOwnerId { get; set; }
        [JsonIgnore]
        public DateTime CreatedDate { get; set; }
        [JsonIgnore]
        public DateTime? EndTime { get; set; }
        [JsonIgnore]
        public string LobbyId { get; set; }
        [JsonIgnore]
        public List<Move> Moves { get; set; }
    }
}
