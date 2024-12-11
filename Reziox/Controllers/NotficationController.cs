
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model;


namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public NotificationsController(AppDbContext db)
        {
            _db = db;

        }

        [HttpGet("Get{userId}")]
        public async Task<IActionResult> Get([FromRoute]int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existnotifications = await _db.Notifications
                    .Where(n => n.UserId == userId)
                    .Where(n => n.CreatedAt.DayOfYear >= DateTime.UtcNow.DayOfYear - 7)
                    //order form new to old
                    .OrderByDescending(n => n.CreatedAt)
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
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
