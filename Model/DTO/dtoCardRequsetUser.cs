using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoCardRequsetUser
    {
        [Required]
        public string City { get; set; }
        [Required]
        public string PlaceName { get; set; }
        [Required]
        public string? BaseImage { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
