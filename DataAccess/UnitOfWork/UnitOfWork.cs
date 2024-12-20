using CloudinaryDotNet;
using DataAccess.ExternalcCloud;
using DataAccess.Repository;
using DataAccess.Repository.IRepository;
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
        public UnitOfWork(AppDbContext db , ICloudImag cloudImag) 
        {
            _db = db;
            
            Users = new UserRepository(_db,cloudImag);
            Places = new PlaceRepository(_db,cloudImag);
        }
        public IUserRepository Users { get;private set; }

        public IPlaceRepository Places { get;private set; }

        public async Task Save()
        {
           await _db.SaveChangesAsync();
        }
    }
}
