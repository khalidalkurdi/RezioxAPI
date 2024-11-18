using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.VisualBasic;
using Reziox.Model.TheUsers;

namespace Reziox.Model.ThePlace
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
        [Required]
        [ForeignKey("user")]
        public int UserId { get; set; }
        [Required]
        [ForeignKey("place")]
        public int PlaceId { get; set; }
        [Required]
        public DateTime BookingDate { get; set; }
        public MyShifts Typeshifts { get; set; }
        /*
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }*/
        [Required]
        public User user { get; set; }
        [Required]
        public Place place { get; set; }
    }
}
