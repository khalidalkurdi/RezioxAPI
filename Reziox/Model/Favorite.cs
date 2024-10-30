using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reziox.Model
{
    public class Favorite
    {
        public int FavoriteId { get; set; }
        [ForeignKey("user")]
        public int UserId { get; set; }
        [ForeignKey("place")]
        public int PlaceId { get; set; }
        public User user { get; set; }
        public Place place { get; set; }
    }
}
