using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reziox.Model.ThePlace
{
    public class PartTime
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("place")]
        public int PlaceId { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public Place place { get; set; }

    }
}
