using Microsoft.AspNetCore.Mvc;
using Model;
using Model.DTO;
using Reziox.DataAccess;

namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportController : ControllerBase
    {
        private readonly AppDbContext _db;
 
        public SupportController(AppDbContext db)
        {
            _db = db;
        }
        [HttpPost("Requset")]
        public async Task<IActionResult> Requset([FromBody] dtoInquiry inquiry)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("not valid");
                }

                await _db.Inquirys.AddAsync(new Inquiry
                {
                    UserId = inquiry.UserId,
                    Message = inquiry.Message,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                return Ok(" we will replay soon .. thank you for using Reziox !");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    }
}
