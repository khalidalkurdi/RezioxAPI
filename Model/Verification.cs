using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Verification
    {

        [Key]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]       
        public string Code { get; set; }
        [Required]
        public DateTime ExDate { get; set; } = DateTime.UtcNow.AddMinutes(3);

    }
}
