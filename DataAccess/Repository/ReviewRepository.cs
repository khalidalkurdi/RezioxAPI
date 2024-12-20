using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model.ThePlace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _db;

        public ReviewRepository(AppDbContext db)
        {
            _db = db;
        }     
        public async Task AddAsync(dtoReview userreview)
        {

           var review = new Review { PlaceId = userreview.PlaceId, UserId = userreview.UserId, Rating = userreview.Rating, Comment = userreview.Comment };
           await _db.Reviews.AddAsync(review);
        }

        public async Task<List<Review>> GetAllAsync()
        {
            var reviews = await _db.Reviews
                                   .Include(r=>r.user)
                                   .Include(r=>r.place)
                                   .ToListAsync();
            return reviews;
        }
    }
}
