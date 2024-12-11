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
        public List<string> ListImage { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        public string PlacePhone { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string LocationUrl { get; set; }
        [Required]       
        public int Visitors { get; set; }
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
        public int Beds { get; set; }
        [Required]
        public int BathRoom { get; set; }
        [Required]
        public int Shower { get; set; }



        
        public bool WiFi { get; set; }
        public bool PaymentByCard { get; set; }
        public bool AirConditioning { get; set; }
        public bool Barbecue { get; set; } 
        public bool EventArea { get; set; }
        public bool ChildrensPlayground { get; set; } = false;      
        public bool ChildrensPool { get; set; }
        public bool Parking { get; set; } 
        public bool Jacuzzi { get; set; } 
        public bool HeatedSwimmingPool { get; set; }         
        public bool Football { get; set; }         
        public bool BabyFoot { get; set; }         
        public bool Ballpool { get; set; }         
        public bool Tennis { get; set; }        
        public bool Volleyball { get; set; } 


    }
}
