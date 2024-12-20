using Microsoft.AspNetCore.Http;
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
    public interface IUserRepository
    {
        Task<User> GetAsync(Expression<Func<User, bool>> filter);
        Task<dtoProfile> GetProfileAsync(Expression<Func<User, bool>> filter);
        Task<List<User>> GetAllAsync();
        Task<dtoProfile> UpdateAsync( dtoUpdateProfile updatedProfile, IFormFile userImage);
        Task<dtoProfile> AddAsync( dtoSignUp dtoSignUp);
    }
}
