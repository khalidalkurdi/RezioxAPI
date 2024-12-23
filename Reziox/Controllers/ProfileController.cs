using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using DataAccess.UnitOfWork;
using DataAccess.Repository.IRepository;

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
        public async Task<IActionResult> Get( [FromRoute]int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var profileUser =await _db.Users.GetProfileAsync(u => u.UserId == userId);
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
                
                var userProfile= await _db.Users.UpdateAsync(updatedProfile, editUserImage);               
                await _db.Save();
                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
