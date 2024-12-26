using Model.ThePlace.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Reziox.Model.ThePlace
{
    public class PlaceImage : BasePlaceImage
    {
        
        [Required]
        [ForeignKey("place")]
        public int? PlaceId { get; set; }
        [Required]
        public MyStatus ImageStatus { get; set; } = MyStatus.approve;
        [Required]        
        public Place place { get; set; }       
    }
}
