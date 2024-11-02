using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model;

namespace Rezioxgithub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet("GetPendingPlace")]
        public async Task<IActionResult> GetPendingPlace()
        {
                var pendinglist = await _db.Places
                .Where(p => p.Status ==Status.Pending )
                .ToListAsync();              
                return Ok(pendinglist);
        }
        [HttpPut("EnablePlace")]
        public async Task<IActionResult> EnablePlace(int id)
        {
            var place = await _db.Places.FirstOrDefaultAsync(p => p.PlaceId == id);            
            if (place == null)
            {
                return NotFound();
            }
            place.Status = Status.Enabled;
            var notification = new Notification
            {
                UserId = place.OwnerId,
                Message = "Accept Your place",
                CreatedAt = DateTime.Now

            };
            await _db.Notifications.AddAsync(notification);
            _db.SaveChangesAsync();
            return Ok();
        }
        [HttpPut("DiseabledPlace")]
        public async Task<IActionResult> DiseabledPlace(int id)
        {
            var place = await _db.Places.FirstOrDefaultAsync(p => p.PlaceId == id);
            if (place == null)
            {
                return NotFound();
            }
            place.Status = Status.Diseabled;
            var notification = new Notification
            {
                UserId = place.OwnerId,
                Message = "Your place is violates our standards",
                CreatedAt = DateTime.Now
            };
            await _db.Notifications.AddAsync(notification);
            _db.SaveChangesAsync();
            return Ok();
        }

    }
}
