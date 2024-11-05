using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Reziox.Model.TheUsers
{
    public class SignUpVM  
    {

        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [PasswordPropertyText]
        [MaxLength(30)]
        [MinLength(8)]
        public string Password { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [Required]
        public string City { get; set; }
    }
}
