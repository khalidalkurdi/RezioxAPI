
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;
using System.Linq;

namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly Cloudinary _cloudinary;
        public ProfileController(AppDbContext db, Cloudinary cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }
        [HttpGet("Get/{userId}")]
        public async Task<IActionResult> Get([FromRoute] int userId)
        {
            var existUser = await _db.Users
                                      .Where(u => u.UserId == userId)
                                      .Include(u => u.Myplaces)
                                      .Include(u=>u.Mybookings)
                                      .FirstOrDefaultAsync();
            if (existUser == null)
            {
                return BadRequest("is not found");
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
                BookingsCanceling=existUser.BookingsCanceling
            };
            return Ok(profileuser);
        }
        [HttpPut("Edit")] // try create class for update
        public async Task<IActionResult> Edit([FromForm] dtoProfile updateUserRequest, IFormFile? edituserimage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (updateUserRequest.UserId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            //find the user by id
            var user = await _db.Users.Where(u => u.UserId == updateUserRequest.UserId).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound($"user {updateUserRequest.UserId} not found.");
            }
            //update user info
            if(updateUserRequest.UserImage!=null)
            {
                var userimage = await SaveImageAsync(edituserimage);
                if (userimage != null)
                {
                    user.UserImage = userimage;
                }
            }
            if (!Enum.TryParse(updateUserRequest.City.ToLower(), out MyCitys cityEnum))
            {
                return BadRequest($"City :{updateUserRequest.City}");
            }
            user.UserName = updateUserRequest.UserName;
            user.Email = updateUserRequest.Email;
            user.PhoneNumber = updateUserRequest.PhoneNumber;
            user.City = cityEnum;
            await _db.SaveChangesAsync();
            return Ok(user);
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
                return null;
            return uploadResult.SecureUrl.ToString();
        }
    }
}
