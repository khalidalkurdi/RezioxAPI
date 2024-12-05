using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model.ThePlace;
using System.Linq.Expressions;

namespace Rezioxgithub.DataAccess.Repository
{
    public class PlaceRepository : IPlaceRepository
    {
        private readonly AppDbContext _db;
        public PlaceRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task Add(dtoAddPlace place, ICollection<IFormFile> placImages)
        {
            throw new NotImplementedException();
        }

        public Task<Place> Get(Expression<Func<Place, bool>> filter, string? includeProperties = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Place>> GetAll(string? includeProperties = null)
        {
            throw new NotImplementedException();
        }

        public void Remove(Expression<Func<Place, bool>> function)
        {
            throw new NotImplementedException();
        }

        public Task Update(Place entity)
        {
            throw new NotImplementedException();
        }
    }
}
