using Microsoft.AspNetCore.Http;
using Model.DTO;
using Reziox.Model.ThePlace;
using System.Linq.Expressions;

namespace DataAccess.Repository.IRepository
{
    public interface IBookingRepository
    {
        public Task<Place> GetAsync(Expression<Func<Place, bool>> filter);
        public Task<dtoDetailsPlace> GetDetailsAsync(Expression<Func<Place, bool>> filter);
        public Task<List<Place>> GetAllAsync();
        public Task<List<dtoCardPlace>> GetRangeAsync(Expression<Func<Place, bool>> filter);
        public Task AddAsync(dtoAddPlace place,ICollection<IFormFile> placImages);
        public Task UpdateAsync(dtoUpdatePlace updatedPlace, ICollection<IFormFile> placImages);
        public Task EnableAsync(Expression<Func<Place, bool>> filter);
        public Task DisableAsync(Expression<Func<Place, bool>> filter);        
    }
}
