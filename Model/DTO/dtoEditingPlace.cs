
using System.ComponentModel.DataAnnotations;

namespace Model.DTO
{
    public class dtoEditingPlace
    {       
        public int? PlaceId { get; set; }
        [Required]
        public int OwnerId { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        [StringLength(10)]
        public string PlacePhone { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string? LocationUrl { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public int Firstpayment { get; set; }
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
        public bool WiFi { get; set; } = false;
        [Required]
        public bool PaymentByCard { get; set; } = false;
        [Required]
        public bool AirConditioning { get; set; } = false;
        [Required]
        public bool Barbecue { get; set; } = false;
        [Required]
        public bool EventArea { get; set; } = false;
        [Required]
        public bool ChildrensPlayground { get; set; } = false;
        [Required]
        public bool ChildrensPool { get; set; } = false;
        [Required]
        public bool Parking { get; set; } = false;
        [Required]
        public bool Jacuzzi { get; set; } = false;
        [Required]
        public bool HeatedSwimmingPool { get; set; } = false;
        [Required]
        public bool Football { get; set; } = false;
        [Required]
        public bool BabyFoot { get; set; } = false;
        [Required]
        public bool Ballpool { get; set; } = false;
        [Required]
        public bool Tennis { get; set; } = false;
        [Required]
        public bool Volleyball { get; set; } = false;
        #endregion
    }
}
