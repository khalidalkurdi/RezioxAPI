using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;
using Reziox.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Rezioxgithub.Model.ThePlace
{
    public class PlaceVM    
    {
        [Required]
        public int OwnerId { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public ICollection<PartTimeVM> partsTime { get; set; }
        [Required]
        public ICollection<string> availabledays { get; set; }
        [Required]
        public ICollection<IFormFile> listimage { get; set; }

    }
}

