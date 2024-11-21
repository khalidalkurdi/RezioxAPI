using System.ComponentModel.DataAnnotations;
namespace Reziox.Model
{
    public class dtoNotification
    {      
        [Required]
        public string Message { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }       
    }
}