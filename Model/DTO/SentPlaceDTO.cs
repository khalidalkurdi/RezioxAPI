using Rezioxgithub.Model.ThePlace;

namespace Rezioxgithub.Model.DTO
{
    public class SentPlaceDTO : PlaceDto
    {
        public ICollection<PlaceImageDTO> placeImages {  get; set; }= new List<PlaceImageDTO>();
    }
}
