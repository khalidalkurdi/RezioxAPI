
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
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _db;
        public NotificationController(AppDbContext db)
        {
            _db = db;

        }

        [HttpGet("Get{userId}")]
        public async Task<IActionResult> Get([FromRoute]int userId)
        {
            if (userId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existnotifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                .Where(n => n.CreatedAt.DayOfYear >= DateTime.Now.DayOfYear - 7)
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

    }
}
