
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.TheUsers;


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
                var existnotifications = await _db.Notifications.AsNoTracking()
                                                                .Where(n => n.UserId == userId)
                                                                .Where(n => n.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                                                                //order form new to old
                                                                .OrderByDescending(n => n.CreatedAt)
                                                                .ToListAsync();

                if (existnotifications.Count == 0)
                {
                    return Ok(existnotifications);
                }
               
                var dtoNotifications = new List<dtoNotification>();
                foreach (var notification in existnotifications)
                {
                    var difDate =  DateTime.UtcNow.AddHours(3) - notification.CreatedAt;
                    var days = difDate.Days > 0 ? $"{difDate.Days} Day " : null;
                    var hours = difDate.Hours > 0 && days == null? $"{difDate.Hours} h " : null;
                    var minutes = difDate.Minutes > 0 && hours== null && days==null? $"{difDate.Minutes} m " : null;
                    var countdown = $"{days}{hours}{minutes}";
                    dtoNotifications.Add(new dtoNotification {NotificationId=notification.NotificationId, Title = notification.Title, Message = notification.Message, CreatedAt = $"{countdown}",IsRead=notification.IsRead, MoveTo=(int)notification.MoveTo });
                }

                return Ok(dtoNotifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Readed/{notificationId}")]
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
                await _db.SaveChangesAsync();
                return Ok("done!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("Alert/{userId}")]
        public async Task<IActionResult> Alert([FromRoute] int userId)
        {
            try
            {
                var existnotifications = await _db.Notifications.AsNoTracking()
                                                                .Where(n => n.UserId == userId)
                                                                .Where(n => n.CreatedAt >= DateTime.UtcNow.AddDays(-7))                                                                
                                                                .ToListAsync();                
                if (existnotifications.Any(n => n.IsRead == false))
                {
                    return Ok(true);
                }
                return Ok(false);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
