using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Reziox.Model.ThePlace;

namespace Model.DTO
{
    public class dtoSignUp
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [PasswordPropertyText]
        [MinLength(8)]
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
