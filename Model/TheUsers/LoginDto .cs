using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Reziox.Model.TheUsers
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [PasswordPropertyText]
        [MaxLength(30)]
        [MinLength(8)]
        public string Password { get; set; }
    }
}
