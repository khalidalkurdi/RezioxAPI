using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using DataAccess.UnitOfWork;

namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IUnitOfWork _db;       
        public ProfileController(IUnitOfWork db)
        {
            _db = db;
           
        }
        [HttpGet("Get/{userId}")]
        public async Task<IActionResult> Get([FromRoute] int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }

                var existUser = await _db.Users.Get((u => u.UserId == userId), "Myplaces , Mybookings");

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
                    BookingsCanceling = existUser.BookingsCanceling
                };
                return Ok(profileuser);
            }catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
                  
        }
        [HttpPut("Edit")]
        public async Task<IActionResult> Edit([FromForm] dtoProfile updatedProfile, IFormFile? editUserImage)
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
                //find the user by id
                var user = await _db.Users.Get((u => u.UserId == updatedProfile.UserId));
                if (user == null)
                {
                    return NotFound($"user {updatedProfile.UserId} not found.");
                }
                await _db.Users.Update(updatedProfile, editUserImage);               
                await _db.Save();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
