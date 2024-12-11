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
using DataAccess.Repository.ExternalcCloud;
using System.Linq.Expressions;

namespace DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        private readonly ICloudImag _cloudImag;
        public UserRepository(AppDbContext db, ICloudImag cloudImag)
        {
            _db = db;
            _cloudImag = cloudImag;
        }

        public async Task<dtoProfile> Get(Expression<Func<User, bool>> filter)
        {

            var existUser = await _db.Users.Where(filter).FirstOrDefaultAsync();

            if (existUser == null)
            {
                throw new Exception("is not found");
            }
            var profileuser = new dtoProfile
            {
                UserId = existUser.UserId,
                UserImage = existUser.UserImage,
                UserName = existUser.UserName,
                Email = existUser.Email,
                PhoneNumber = existUser.PhoneNumber,
                City = existUser.City.ToString(),
                UserPlaces = existUser.Places,
                UserBookings = existUser.Bookings,
                BookingsCanceling = existUser.BookingsCanceling
            };
            return profileuser;
        }

        public async Task<dtoProfile> Update( dtoProfile updatedProfile, IFormFile userImage)
        {
            //find the user by id
            var existuser = await _db.Users.Where(u => u.UserId == updatedProfile.UserId).FirstOrDefaultAsync();
            if (existuser == null)
            {
                throw new Exception ($"user {updatedProfile.UserId} not found.");
            }
            if (userImage != null)
            {
                var imageUrl = await _cloudImag.SaveImageAsync(userImage);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    throw new Exception("internal error when try to upload image");
                }
                    existuser.UserImage = imageUrl;
            }

            if (!Enum.TryParse(updatedProfile.City, true, out MyCitys cityEnum))
            {
                throw new Exception($"invalid city : {updatedProfile.City}");
            }

            existuser.UserName = updatedProfile.UserName;
            existuser.Email = updatedProfile.Email;
            existuser.PhoneNumber = updatedProfile.PhoneNumber;
            existuser.City = cityEnum;
            return await Get(u => u.UserId == updatedProfile.UserId);
        }

    }
}
