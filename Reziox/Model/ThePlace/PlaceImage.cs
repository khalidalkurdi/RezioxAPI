using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Reziox.Model.ThePlace
{
    public class PlaceImage
    {
        [Key]
        public int ImageId { get; set; }
        [Required]
        [ForeignKey("place")]
        public int PlaceId { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        [JsonIgnore]
        public Place place { get; set; }
    }
}
