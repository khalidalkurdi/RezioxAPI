using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Reziox.Model
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }
        [ForeignKey("user")]
        [Required]
        public int UserId { get; set; }
        [Required]
        [ForeignKey("place")]
        public int PlaceId { get; set; }
        [Required]
        [JsonIgnore]
        public User user { get; set; }
        [Required]
        public Place place { get; set; }
    }
}
