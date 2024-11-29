using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoDetailsPlace
    {
        [Required]
        public int PlaceId { get; set; }
        [Required]
        public List<string> ListImage { get;}
        [Required]
        public string PlaceName { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string LocationUrl { get; set; }
        [Required]       
        public int Visitors { get; set; }
        [Required]
        public string PlacePhone { get; set; }
        [Required]
        public double Rating { get; set; }
        [Required]
        public int CountReviews { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int MasterRoom { get; set; }
        [Required]
        public int BedRoom { get; set; }
        [Required]
        public int AllBeds { get; set; }
        [Required]
        public int BathRoom { get; set; }
        [Required]
        public int Shower { get; set; }
        [Required]
        public List<string> Features { get; set; }

    }
}
