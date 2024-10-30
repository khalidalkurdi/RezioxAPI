using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reziox.Model.ThePlace
{
    public class AvailableDay
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("place")]
        public int PlaceId { get; set; }

        [Required]
        public DaysofWeek Day { get; set; } // Enum to store the day (e.g., Monday)

        public Place place { get; set; } // Navigation property to the Place entity
    }
}
