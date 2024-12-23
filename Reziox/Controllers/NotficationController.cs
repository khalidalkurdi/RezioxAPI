
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

        [HttpGet("Gets/{userId}")]
        public async Task<IActionResult> Gets([FromRoute]int userId)
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

                if (existnotifications.Count == 0)
                {
                    return NotFound("no notifications found for this user.");
                }
                var dtoNotifications = new List<dtoNotification>();
                foreach (var notification in existnotifications)
                {
                    var difDate = notification.CreatedAt - DateTime.UtcNow;
                    var countdown = difDate.Days < 0 ? $"{difDate.Days} day " : $"{difDate.Hours} hour";
                    dtoNotifications.Add(new dtoNotification {NotificationId=notification.NotificationId, Title = notification.Title, Message = notification.Message, CreatedAt = $"{countdown}",IsRead=notification.IsRead });
                }

                return Ok(dtoNotifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("Readed/{notificationId}")]
        public async Task<IActionResult> Readed([FromRoute] int notificationId)
        {
            try
            {
                if (notificationId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existNotification = await _db.Notifications
                                                    .Where(n => n.NotificationId == notificationId)
                                                    .FirstOrDefaultAsync();

                if (existNotification == null)
                {
                    return NotFound("is not found");
                }
                existNotification.IsRead=true;
                _db.SaveChanges();
                return Ok("done!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
