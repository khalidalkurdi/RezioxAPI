using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Reziox.Model.ThePlace;

namespace Reziox.Model.TheUsers
{
    public class SignUpDto  
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
        public string City { get; set; }
    }
}
