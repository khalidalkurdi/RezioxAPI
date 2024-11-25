using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoCardPlace
    {
        [Required]
        public int PlaceId { get; set; }
        [Required]
        public string BaseImage { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public int Visitors { get; set; }
        [Required]
        public double Rating { get; set; }
        
    }
}
