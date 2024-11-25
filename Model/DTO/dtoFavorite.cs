using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoFavorite
    {
        [Required]
        public int PlaceId { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        public string BaseImageUrl { get; set; }
    }
}
