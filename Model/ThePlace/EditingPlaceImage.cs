using Model.ThePlace.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Reziox.Model.ThePlace
{
    public class EditingPlaceImage : BasePlaceImage
    {
        [Required]
        [ForeignKey("editingplace")]
        public int EditingPlaceId { get; set; }        
        [Required]
        public EditingPlace editingplace { get; set; }
    }
}
