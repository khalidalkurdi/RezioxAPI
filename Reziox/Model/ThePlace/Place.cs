using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public Citys City { get; set; }
        [Required]
        public Status Status { get; set; } = Status.Pending;
        [Required]
        public string Description { get; set; }
        [Required]
        public int Price { get; set; }
        [NotMapped]
        public double Rating => reviews != null ? reviews.Average(r => r.Rating) : 0.0;
        public ICollection<PartTime> partsTime { get; set; }
        public ICollection<PlaceImage> listimage { get; set; }
        public ICollection<Booking> listbookings { get; set; }
        public ICollection<Review> reviews { get; set; }
        public ICollection<AvailableDay> availabledays { get; set; }
        public User user { get; set; }
        
    }
}
