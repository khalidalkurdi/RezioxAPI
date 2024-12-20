using Microsoft.AspNetCore.Http;
using Model;
using Model.DTO;
using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface ISupportRepository
    {
        Task<List<Support>> GetAllAsync();
        public Task<List<Support>> GetRangeAsync(Expression<Func<Support, bool>> filter);
        Task AddAsync(dtoSupport dtoISupport);
    }
}
