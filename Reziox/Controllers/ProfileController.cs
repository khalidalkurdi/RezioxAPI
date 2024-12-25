using DataAccess.ExternalcCloud;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;

namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;       
        private readonly ICloudImag _cloudImag;       
        public ProfileController(AppDbContext db ,ICloudImag cloudImag)
        {
            _db = db;
            _cloudImag = cloudImag;
        }
        [HttpGet("Get/{userId}")]
        public async Task<IActionResult> Get([FromRoute]int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existUser = await _db.Users.AsNoTracking()
                                                .Where(u => u.UserId == userId)
                                               .Include(u => u.Myplaces)
                                               .Include(u => u.Mybookings)
                                               .FirstOrDefaultAsync();
                if (existUser == null)
                {
                    return NotFound("user is not found");
                }
                var profileUser = new dtoProfile
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
                return Ok(profileUser);
            }catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
                  
        }
        [HttpPut("Edit")]
        public async Task<IActionResult> Edit([FromForm] dtoUpdateProfile updatedProfile, IFormFile? editUserImage)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (updatedProfile.UserId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existuser = await _db.Users.Where(u => u.UserId == updatedProfile.UserId).FirstOrDefaultAsync();
                if (existuser == null)
                {
                    return NotFound($"user {updatedProfile.UserId} not found.");
                }
                if (editUserImage != null)
                {
                    var imageUrl = await _cloudImag.SaveImageAsync(editUserImage);
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        return BadRequest("internal error when try to upload image");
                    }
                    existuser.UserImage = imageUrl;
                }

                if (!Enum.TryParse(updatedProfile.City, true, out MyCitys cityEnum))
                {
                    return BadRequest($"invalid city : {updatedProfile.City}");
                }

                existuser.UserName = updatedProfile.UserName;
                existuser.Email = updatedProfile.Email;
                existuser.PhoneNumber = updatedProfile.PhoneNumber;
                existuser.City = cityEnum;

                await _db.SaveChangesAsync();
                return Ok(Get(existuser.UserId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
