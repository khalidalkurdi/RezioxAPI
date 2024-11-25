
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
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly Cloudinary _cloudinary;
        public UserController(AppDbContext db, Cloudinary cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
        }
        
        [HttpGet("GetNotifications{userId}")]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            if (userId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existnotifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                .Where(n=>n.CreatedAt.DayOfYear>=DateTime.Now.DayOfYear-7)
                //order form new to old
                .OrderBy(n => n.CreatedAt)
                .ToListAsync();

            if (existnotifications == null)
            {
                return NotFound("no notifications found for this user.");
            }
            var notifications = new List<dtoNotification>();
            foreach (var n in existnotifications)
            {
                notifications.Add(new dtoNotification { Message = n.Message, CreatedAt = n.CreatedAt });
            }
            
            return Ok(notifications);
        }

        [HttpPost("AddReview{placeId}")]
        public async Task<IActionResult> AddReview(int userId, int placeId, int rating)
        {
            if(userId==0 || placeId == 0)
            {
                return BadRequest("0 id is not correct");
            }
            // Check if User and Place exist
            var existuser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            var existplace = await _db.Places.FirstOrDefaultAsync(p => p.PlaceId == placeId);
            if (existuser == null || existplace == null)
            {
                return NotFound("user or place not found");
            }
            var existbokking = await _db.Bookings
                                        .Where(f => f.UserId == userId)
                                        .Where(f => f.PlaceId == placeId)
                                        .FirstOrDefaultAsync();
            if(existbokking == null || existbokking.BookingDate.DayOfYear>DateTime.Today.DayOfYear)
            {
                return BadRequest("can not review this place !");
            }
            var review = new Review { PlaceId = placeId, UserId = userId, Rating = rating};
            existplace.ListReviews.Add(review);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("GetProfile {userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var existUser = await _db.Users
                                      .Where(u => u.UserId == userId)
                                      .Include(u => u.Myplaces)
                                      .FirstOrDefaultAsync();
            if (existUser == null)
            {
                return NotFound("is not found");
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
                UserBookings = existUser.Bookings
            };
            return Ok(profileuser);
        }
        [HttpPut("EditProfile")] // try create class for update
        public async Task<IActionResult> EditProfile([FromBody] dtoProfile updateUserRequest, IFormFile? edituserimage)
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
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == updateUserRequest.UserId);
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

        [HttpPost("Support")]
        public async Task<IActionResult> Support( [FromBody]dtoInquiry inquiry )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("not valid");
            }

            await _db.Inquirys.AddAsync(new Inquiry
            {
                UserId=inquiry.UserId,
                Message = inquiry.Message,
                CreatedAt= DateTime.Now
            });
            await _db.SaveChangesAsync();
            return Ok(" we will replay soon .. thank you for using Reziox !");
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
