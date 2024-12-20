using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Reziox.DataAccess;
using Reziox.Model.TheUsers;
using Rezioxgithub.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reziox.Model;
using Model.DTO;
using Microsoft.EntityFrameworkCore;
using DataAccess.Repository.IRepository;
using System.Linq.Expressions;
using DataAccess.ExternalcCloud;
using BCrypt.Net;
using Reziox.Model.ThePlace;
using Model;

namespace DataAccess.Repository
{
    public class SupportRepository : ISupportRepository
    {
        private readonly AppDbContext _db;
        private object existUsers;

        public SupportRepository(AppDbContext db)
        {
            _db = db;           
        }
        public async Task AddAsync(dtoSupport support)
        {
            await _db.Supports.AddAsync(new Support
            {
                UserId = support.UserId,
                Message = support.Message,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task<List<Support>> GetAllAsync()
        {
            var existSupports = await _db.Supports
                                      .Include(s =>s.user )
                                      .OrderBy(s => s.CreatedAt)                   
                                      .ToListAsync();
            if (existUsers == null)
            {
                throw new Exception("is not found");
            }
            return existSupports;
        }

        public async Task<List<Support>> GetRangeAsync(Expression<Func<Support, bool>> filter)
        {
            var existSupports = await _db.Supports.Where(filter)                              
                                                  .OrderBy(s => s.CreatedAt)
                                                  .ToListAsync();
            if (existUsers == null)
            {
                throw new Exception("is not found");
            }
            return existSupports;
        }
    }
}
