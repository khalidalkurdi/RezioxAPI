using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Reziox.Model.ThePlace;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Reziox.Model.TheUsers
{//class user
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
        [Required]
        public Citys City { get; set; }
        [ValidateNever]
        public ICollection<Booking> mybookings { get; set; }
        [ValidateNever]
        public ICollection<Place> myplaces { get; set; }
        [ValidateNever]
        public ICollection<Favorite> myfavorites { get; set; }


    }
}
