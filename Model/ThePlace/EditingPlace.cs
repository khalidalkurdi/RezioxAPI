using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;
using Reziox.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Model.ThePlace.Base;

namespace Reziox.Model.ThePlace
{
    public class EditingPlace : BasePlace
    {
        public EditingPlace()
        {
            PlaceStatus = MyStatus.pending;
        }
        [Key]
        public int EditingPlaceId { get; set; }
        public int? PlaceId { get; set; }                
        public ICollection<EditingPlaceImage> Listimage { get; set; } = new List<EditingPlaceImage>();        
    }
}
