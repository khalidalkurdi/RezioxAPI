﻿
using Reziox.Model.ThePlace;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Reziox.Model.TheUsers
{
    public class User
    {
        [Key]
        public int UserId { get; set; }       
        [Required]
        public string DiviceToken { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [PasswordPropertyText]
        public string Password { get; set; }
        [Required]
        [Phone]
        [StringLength(10)]
        public string PhoneNumber { get; set; }
        public string? UserImage { get; set; }
        [Required]
        public MyCitys City { get; set; }
        [NotMapped]
        public int Places => Myplaces.Count == 0 ? 0 :Myplaces.Where(p=>p.PlaceStatus==MyStatus.approve).Count();
        [NotMapped]
        public int Bookings => Mybookings.Count == 0 ? 0 : Mybookings.Where(p => p.StatusBooking == MyStatus.confirmation).Count();
        [NotMapped]
        public int BookingsCanceling => Mybookings.Count()==0 ? 0 : Mybookings.Where(p => p.StatusBooking == MyStatus.cancel).Count();

        public ICollection<Booking> Mybookings { get; set; } = new List<Booking>();
        public ICollection<Place> Myplaces { get; set; }=new List<Place>();
        public ICollection<Favorite> Myfavorites { get; set; }= new List<Favorite>();
        public ICollection<Notification> MyNotifications { get; set; }= new List<Notification>();
    }
}
