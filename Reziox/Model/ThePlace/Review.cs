using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reziox.Model.TheUsers;

namespace Reziox.Model.ThePlace
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        [Required]
        [ForeignKey("user")]
        public int UserId { get; set; }
        [Required]
        [ForeignKey("place")]
        public int PlaceId { get; set; }
        [Range(0.0, 5.0)]
        [Required]
        public double Rating { get; set; }
        [Required]
        public User user { get; set; }
        [Required]
        public Place place { get; set; }
    }
}
