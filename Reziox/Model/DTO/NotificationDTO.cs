using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Reziox.Model.TheUsers;

namespace Reziox.Model
{
    public class NotificationDTO
    {      
        [Required]
        public string Message { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }       
    }
}