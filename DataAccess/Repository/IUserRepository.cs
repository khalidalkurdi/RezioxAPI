using Reziox.Model.TheUsers;
using Rezioxgithub.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        void Update(User user);
    }
}
