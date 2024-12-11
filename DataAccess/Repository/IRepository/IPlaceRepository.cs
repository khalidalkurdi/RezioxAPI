using Microsoft.AspNetCore.Http;
using Model.DTO;
using Reziox.Model.ThePlace;
using System.Linq.Expressions;

namespace DataAccess.Repository.IRepository
{
    public interface IPlaceRepository
    {
        Task<IEnumerable<dtoCardPlace>> GetRange(Expression<Func<Place, bool>> filter);
        Task<dtoDetailsPlace> Get(Expression<Func<Place, bool>> filter);
        Task Add(dtoAddPlace place,ICollection<IFormFile> placImages);
        Task Update(dtoUpdatePlace updatedPlace, ICollection<IFormFile> placImages);
        Task Enable(Expression<Func<Place, bool>> filter);
        Task Disable(Expression<Func<Place, bool>> filter);
        Task<List<dtoCardPlace>> CreateCardPlaces(List<Place> places);
        

    }
}
