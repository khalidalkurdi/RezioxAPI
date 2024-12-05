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

namespace DataAccess.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly AppDbContext _db;
        private readonly Cloudinary _cloudinary;
        public UserRepository(AppDbContext db, Cloudinary cloudinary) : base(db)
        {
            _db = db;
            _cloudinary = cloudinary;
        }

        public async Task Update( dtoProfile updatedProfile, IFormFile userImage)
        {
            //find the user by id
            var user = await _db.Users.Where(u => u.UserId == updatedProfile.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new Exception ($"user {updatedProfile.UserId} not found.");
            }
            if (userImage != null)
            {
                var imageUrl = await SaveImageAsync(userImage);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    throw new Exception("internal error when try to upload image");
                }
                    user.UserImage = imageUrl;
            }

            if (!Enum.TryParse(updatedProfile.City, true, out MyCitys cityEnum))
            {
                throw new Exception($"invalid city : {updatedProfile.City}");
            }

            user.UserName = updatedProfile.UserName;
            user.Email = updatedProfile.Email;
            user.PhoneNumber = updatedProfile.PhoneNumber;
            user.City = cityEnum;            
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return null;
            //requst
            using var stream = image.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(image.FileName, stream)
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                return null;
            }
                   
            return uploadResult.SecureUrl.ToString();            
        }

        
    }
}
