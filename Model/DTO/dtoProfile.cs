using Model.DTO.Base;
using Reziox.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Model.DTO
{
    public class dtoProfile : dtoBaseProfile
    {
        public string? UserImage { get; set; }        
        [Required]
        public int UserPlaces { get; set; }
        [Required]
        public int UserBookings { get; set; }
        [Required]
        public int BookingsCanceling {  get; set; }

    }
}
