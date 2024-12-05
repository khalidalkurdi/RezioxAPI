using Microsoft.AspNetCore.Http;
using Model.DTO;
using Reziox.Model.ThePlace;
using System.Linq.Expressions;

namespace DataAccess.Repository.IRepository
{
    public interface IPlaceRepository
    {
        Task Update(Place entity);
        Task<IEnumerable<Place>> GetAll(string? includeProperties = null);
        Task<Place> Get(Expression<Func<Place, bool>> filter, string? includeProperties = null);
        Task Add(dtoAddPlace place,ICollection<IFormFile> placImages);
        void Remove(Expression<Func<Place, bool>> function);       

    }
}
