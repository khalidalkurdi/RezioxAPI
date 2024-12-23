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
        public async Task<IActionResult> Requset([FromBody] dtoSupport dtoSupport)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("not valid");
                }

                await _db.Supports.AddAsync(new Support
                {
                    UserId = dtoSupport.UserId,
                    Complaint = dtoSupport.Complaint,
                    ComplaintType=dtoSupport.ComplaintType
                });
                await _db.SaveChangesAsync();
                return Ok(" we will replay soon .. thank you for using Reziox !");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("Get/{UserId}")]
        public async Task<IActionResult> Get([FromRoute] int UserId)
        {
            try
            {
                if (UserId==0)
                {
                    return BadRequest("not valid");
                }

                var existsupports = await _db.Supports
                                              .Where(s=>s.UserId==UserId)
                                              .OrderBy(s=>s.SupportId)
                                              .ToListAsync();
                if (existsupports.Count==0) 
                { 
                    return Ok(existsupports);
                }
                var cardSupports = new List<dtoCardSuport>();
                foreach (var suport in existsupports)
                {
                    cardSupports.Add(
                        new dtoCardSuport
                        {
                            Complaint= suport.Complaint,
                            Response= suport.Response
                        }
                        );
                }
                return Ok(cardSupports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
