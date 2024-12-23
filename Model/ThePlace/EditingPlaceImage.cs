
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Reziox.Model.ThePlace
{
    public class EditingPlaceImage
    {
        [Key]
        public int ImageId { get; set; }
        [Required]
        [ForeignKey("editingplace")]
        public int EditingPlaceId { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public EditingPlace editingplace { get; set; }
    }
}
