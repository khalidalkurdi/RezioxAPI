using DataAccess.Repository;
using Reziox.DataAccess;
using Rezioxgithub.DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db; 
        public UnitOfWork(AppDbContext db) 
        {
            _db = db;
            Users = new UserRepository(_db);
            Places = new PlaceRepository(_db);
        }
        public IUserRepository Users { get;private set; }

        public IPlaceRepository Places { get;private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
