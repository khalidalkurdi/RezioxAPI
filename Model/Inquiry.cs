using Reziox.Model.TheUsers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model
{
    public class Inquiry
    {

        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("user")]
        public int UserId { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]       
        public User user { get; set; }
    }
}
