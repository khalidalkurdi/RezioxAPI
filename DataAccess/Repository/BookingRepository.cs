using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model.ThePlace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;

        public BookingRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task AddAsync(dtoAddPlace place, ICollection<IFormFile> placImages)
        {
            throw new NotImplementedException();
        }

        public Task DisableAsync(Expression<Func<Place, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task EnableAsync(Expression<Func<Place, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<List<Place>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Place> GetAsync(Expression<Func<Place, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<dtoDetailsPlace> GetDetailsAsync(Expression<Func<Place, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task<List<dtoCardPlace>> GetRangeAsync(Expression<Func<Place, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(dtoUpdatePlace updatedPlace, ICollection<IFormFile> placImages)
        {
            throw new NotImplementedException();
        }
    }
}
