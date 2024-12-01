
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
            if (!ModelState.IsValid)
            {
                return BadRequest("not valid");
            }

            await _db.Inquirys.AddAsync(new Inquiry
            {
                UserId = inquiry.UserId,
                Message = inquiry.Message,
                CreatedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
            return Ok(" we will replay soon .. thank you for using Reziox !");
        }



    }
}
