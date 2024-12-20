using Reziox.Model.TheUsers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model.DTO
{
    public class dtoSupport
    {
        [Required]       
        public int UserId { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
