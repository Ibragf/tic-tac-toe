using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace tic_tac_toe.Models
{
    public class Move
    {
        [Key]
        [JsonIgnore]
        public string Id { get; set; }
        [Required]
        public string PlayerId { get; set; }
        [Required]
        [Range(0, 2)]
        public int X { get; set; }
        [Required]
        [Range(0,2)]
        public int Y { get; set; }
        [Required]
        public string GameId { get; set; }
        [JsonIgnore]
        public DateTime CreatedDate { get; set; }
    }
}
