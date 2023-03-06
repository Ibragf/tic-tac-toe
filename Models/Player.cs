using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace tic_tac_toe.Models
{
    public class Player
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public string ConnectionId { get; set; }
        [JsonIgnore]
        public List<Lobby> Lobbies { get; set;}
        [JsonIgnore]
        public List<Score> Scores { get; set;}
        [JsonIgnore]
        public List<Move> Moves { get; set;}
    }
}
