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
using System.Linq.Expressions;
using DataAccess.ExternalcCloud;
using BCrypt.Net;
using Reziox.Model.ThePlace;

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

        public async Task<dtoProfile> AddAsync(dtoSignUp signUpRequest)
        {
            //checked if the email already exists
            var existemail = await _db.Users.Where(u => u.Email == signUpRequest.Email.ToLower()).FirstOrDefaultAsync();
            if (existemail != null)
            {
                throw new Exception("email is already in use , change it.");
            }
            //convert password to hash for more scurity 
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(signUpRequest.Password);


            //convert string to enum value
            if (!Enum.TryParse(signUpRequest.City.ToLower(), out MyCitys cityEnum))
            {
                throw new Exception($"City :{signUpRequest.City}");
            }
            var user = new User
            {
                UserName = signUpRequest.UserName,
                Email = signUpRequest.Email.ToLower(),
                Password = hashedPassword,
                PhoneNumber = signUpRequest.PhoneNumber,
                City = cityEnum
            };
            //add user
            await _db.Users.AddAsync(user);           
            var profileuser = new dtoProfile
            {
                UserId = user.UserId,
                UserImage = user.UserImage,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                City = user.City.ToString(),
            };
            return profileuser;
        }

        public async Task<User> GetAsync(Expression<Func<User, bool>> filter)
        {
            //checked if exists
            var existUser = await _db.Users.Where(filter).FirstOrDefaultAsync();

            if (existUser == null)
            {
                throw new Exception("user is not found");
            }
            return existUser;
        }
        public async Task<dtoProfile> GetProfileAsync(Expression<Func<User, bool>> filter)
        {

            var existUser = await _db.Users.Where(filter).FirstOrDefaultAsync();

            if (existUser == null)
            {
                throw new Exception("user is not found");
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

        public async Task<List<User>> GetAllAsync()
        {
            var existUsers = await _db.Users
                                      .OrderBy(u => u.UserId)
                                      .Include(u => u.Myplaces)
                                      .Include(u => u.Mybookings)
                                      .ToListAsync();
            if (existUsers == null)
            {
                throw new Exception("is not found");
            }
            return existUsers;
        }

        public async Task<dtoProfile> UpdateAsync( dtoUpdateProfile updatedProfile, IFormFile userImage)
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
            return await GetProfileAsync(u => u.UserId == updatedProfile.UserId);
        }

        
    }
}
