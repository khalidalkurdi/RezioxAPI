using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Reziox.Model.TheUsers;

namespace Reziox.Model.ThePlace
{
    public class Place
    {
        [Key]
        public int PlaceId { get; set; }
        [Required]
        [ForeignKey("user")]
        public int OwnerId { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        public MyCitys City { get; set; }
        [Required]
        public MyStatus Status { get; set; } = MyStatus.pending;
        [Required]
        public string Description { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public MYDays WorkDays { get; set; }
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
        public bool ChildrensPool { get; set;}
        [Required]
        public bool Parking { get; set;}
        [Required]
        public bool Jacuzzi { get; set;}
        [Required]
        public bool HeatedSwimmingPool { get; set;}
        [Required]
        public bool Football { get; set;}
        [Required]
        public bool BabyFoot { get; set;}
        [Required]
        public bool Ballpool { get; set;}
        [Required]
        public bool Tennis { get; set;}
        [Required]
        public bool Volleyball { get; set;}
        #endregion

        //AVG of rating
        [NotMapped]
        [Range(0.0, 5.0)]
        public double Rating =>ListReviews.Count==0 ? 0.0 : ListReviews.Average(r => r.Rating);
        //count reviews
        [NotMapped]
        public int CountRating => ListReviews.Count;       
        public ICollection<PlaceImage> Listimage { get; set; }= new List<PlaceImage>();
        public ICollection<Booking> Listbookings { get; set; }=new List<Booking>();
        public ICollection<Review> ListReviews { get; set; } = new List<Review>();
        [Required]
        public  User user { get; set; }
        
    }
}
