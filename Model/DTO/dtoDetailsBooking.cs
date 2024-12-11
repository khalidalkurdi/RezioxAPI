using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;
using Reziox.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoDetailsBooking
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int PlaceId { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        public string PlacePhone { get; set; }
        [Required]
        public string BookingDate { get; set; }
        [Required]
        public string Time { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public int MaxGust { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]

        public string UserPhone { get; set; }        
        
       
    }



}
