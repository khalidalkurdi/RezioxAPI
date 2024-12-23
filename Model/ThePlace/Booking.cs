using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
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
        public MyStatus StatusBooking { get; set; } = MyStatus.pending;
        [Required]        
        public User user { get; set; }
        [Required]
        
        public Place place { get; set; }
    }
}
