using Reziox.Model.ThePlace;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Rezioxgithub.Model.DTO
{
    public class PlaceImageDTO
    {
        [Key]
        public int ImageId { get; set; }
        [Required]
        public string ImageUrl { get; set; }
    }
}
