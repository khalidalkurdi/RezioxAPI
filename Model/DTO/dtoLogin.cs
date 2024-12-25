using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Model.DTO
{
    public class dtoLogin
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [PasswordPropertyText]
        [MaxLength(30)]
        [MinLength(8)]
        public string Password { get; set; }
        [Required]
        public string DiviceToken { get; set; }
    }
}
