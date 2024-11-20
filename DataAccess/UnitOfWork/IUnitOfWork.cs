using DataAccess.Repository;
using Rezioxgithub.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IPlaceRepository Places  { get; }
        void Save();
    }
}
