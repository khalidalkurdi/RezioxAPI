using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoCardRequsetBooking
    {
        [Required]
        public int BookingId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public int PlaceId { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        public string BaseImage { get; set; }
        [Required]
        public string BookingDate { get; set; }
        [Required]
        public string Time { get; set; }
    }
}
