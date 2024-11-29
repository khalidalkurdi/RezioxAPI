﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoCardBookingSchedule
    {
        [Required]
        public int BookingId { get; set; }
        [Required]
        public string BaseImage { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        public string BookingDate { get; set; }
        [Required]
        public string CountDown { get; set; }
        [Required]
        public string Time{ get; set; }
        
    }
}