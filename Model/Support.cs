using Reziox.Model.TheUsers;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Model
{
    public class Support
    {

        [Key]
        public int SupportId { get; set; }
        [Required]
        [ForeignKey("user")]
        public int UserId { get; set; }
        [Required]
        public string Complaint { get; set; }
        [Required]
        public string ComplaintType { get; set; }
        
        public string? Response { get; set; }
        [Required]       
        public User user { get; set; }
    }
}
