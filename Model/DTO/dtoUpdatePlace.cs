using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoUpdatePlace
    {
        [Required]
        public int PlaceId { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        [StringLength(10)]
        public string PlacePhone { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string LocationUrl { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public List<string> WorkDays { get; set; }
        [Required]
        public int MorrningShift { get; set; }
        [Required]
        public int NightShift { get; set; }
        [Required]
        public int Visitors { get; set; }
        [Required]
        public int Beds { get; set; }
        [Required]
        public int MasterRoom { get; set; }
        [Required]
        public int BedRoom { get; set; }
        [Required]
        public int BathRoom { get; set; }
        [Required]
        public int Shower { get; set; }
        #region  features
        [Required]
        public bool WiFi { get; set; }
        [Required]
        public bool PaymentByCard { get; set; }
        [Required]
        public bool AirConditioning { get; set; }
        [Required]
        public bool Barbecue { get; set; }
        [Required]
        public bool EventArea { get; set; }
        [Required]
        public bool ChildrensPlayground { get; set; }
        [Required]
        public bool ChildrensPool { get; set; }
        [Required]
        public bool Parking { get; set; }
        [Required]
        public bool Jacuzzi { get; set; }
        [Required]
        public bool HeatedSwimmingPool { get; set; }
        [Required]
        public bool Football { get; set; }
        [Required]
        public bool BabyFoot { get; set; }
        [Required]
        public bool Ballpool { get; set; }
        [Required]
        public bool Tennis { get; set; }
        [Required]
        public bool Volleyball { get; set; }
        #endregion
    }
}
