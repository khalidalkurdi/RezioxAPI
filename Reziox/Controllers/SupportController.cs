using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Requset([FromBody] dtoSupport support)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("not valid");
                }

                await _db.Supports.AddAsync(new Support
                {
                    UserId = support.UserId,
                    Message = support.Message,
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
        [HttpGet("Get")]
        public async Task<IActionResult> Get([FromBody] int UserId)
        {
            try
            {
                if (UserId==0)
                {
                    return BadRequest("not valid");
                }

                var existsupports = await _db.Supports
                                              .Where(s=>s.UserId==UserId)
                                              .OrderBy(s=>s.CreatedAt)
                                              .ToListAsync();
                if (existsupports.Count==0) 
                { 
                    return NotFound("was not found");
                }
                return Ok(existsupports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    }
}
