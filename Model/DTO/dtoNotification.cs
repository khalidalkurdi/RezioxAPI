using System.ComponentModel.DataAnnotations;
namespace Reziox.Model
{
    public class dtoNotification
    {
        [Key]
        public int NotificationId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public string CreatedAt { get; set; }
        [Required]
        public bool IsRead { get; set; }
    }
}