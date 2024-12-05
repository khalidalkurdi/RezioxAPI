using CloudinaryDotNet;
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
        private readonly Cloudinary _cloudinary; 

        public UnitOfWork(AppDbContext db, Cloudinary cloudinary) 
        {
            _db = db;
            _cloudinary = cloudinary;
            Users = new UserRepository(_db,_cloudinary);
            Places = new PlaceRepository(_db);
        }
        public IUserRepository Users { get;private set; }

        public IPlaceRepository Places { get;private set; }

        public async Task Save()
        {
           await _db.SaveChangesAsync();
        }
    }
}
