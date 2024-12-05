using Microsoft.AspNetCore.Http;
using Model.DTO;
using Reziox.Model.TheUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface IUserRepository : IRepository<User>
    {
        Task Update( dtoProfile updatedProfile, IFormFile userImage);
    }
}
