using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Reziox.Model.TheUsers;

namespace Reziox.Model
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("user")]
        public int UserId { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        [JsonIgnore]
        public User user { get; set; }
    }
}