using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Model.DTO;
using Model.ThePlace.Base;
using Reziox.Model.TheUsers;

namespace Reziox.Model.ThePlace
{
    public class Place : BasePlace
    {
        public Place()
        {
            PlaceStatus = MyStatus.approve;
        }
        [Key]
        public int PlaceId { get; set; }       
        //AVG of rating
        [NotMapped]
        [Range(0.0, 5.0)]
        public double Rating =>ListReviews.Count==0 ? 0.0 : ListReviews.Average(r => r.Rating);
        //count reviews
        [NotMapped]
        public int CountReviews => ListReviews.Count;       
        public ICollection<PlaceImage> Listimage { get; set; }= new List<PlaceImage>();
        public ICollection<Booking> Listbookings { get; set; }=new List<Booking>();
        public ICollection<Review> ListReviews { get; set; } = new List<Review>();
       

    }
}
