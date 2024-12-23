using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoCardSuport
    {
        [Required]
        public string Complaint { get; set; }
        [Required]
        public string Response { get; set; }
    }
}
