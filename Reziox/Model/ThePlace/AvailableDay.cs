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
        public DaysofWeek Day { get; set; } 

        public Place place { get; set; }
    }
}
