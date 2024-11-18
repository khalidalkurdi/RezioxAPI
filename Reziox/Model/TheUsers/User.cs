using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Reziox.Model.ThePlace;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Reziox.Model.TheUsers
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
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
        public ICollection<Booking> Mybookings { get; set; } = new List<Booking>();
        public ICollection<Place> Myplaces { get; set; }=new List<Place>();
        public ICollection<Favorite> Myfavorites { get; set; }= new List<Favorite>();
        public ICollection<Notification> MyNotifications { get; set; }= new List<Notification>();
    }
}
