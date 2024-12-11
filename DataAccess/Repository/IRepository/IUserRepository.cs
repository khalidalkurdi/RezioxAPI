using Microsoft.AspNetCore.Http;
using Model.DTO;
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
        Task<dtoProfile> Get(Expression<Func<User, bool>> function);
        Task<dtoProfile> Update( dtoProfile updatedProfile, IFormFile userImage);
    }
}
