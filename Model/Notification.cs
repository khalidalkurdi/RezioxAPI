using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Reziox.Model.TheUsers;

namespace Reziox.Model
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }
        [Required]
        [ForeignKey("user")]
        public int UserId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }= DateTime.UtcNow.AddHours(3);
        [Required]
        public bool IsRead { get; set; }
        [Required]
        public MyScreen MoveTo { get; set; }
        [Required]
        public User user { get; set; }
    }
}