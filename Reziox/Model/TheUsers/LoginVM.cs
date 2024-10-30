using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Reziox.Model.TheUsers
{
    public class LoginVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [PasswordPropertyText]
        public string Password { get; set; }
    }
}
