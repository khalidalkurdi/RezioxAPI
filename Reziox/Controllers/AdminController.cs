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
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var existUsers = await _db.Users                                                                            
                                      .OrderBy(u => u.UserId)
                                      .ToListAsync();
            if (existUsers == null)
            {
                return NotFound("is not found");
            }
            return Ok(existUsers);
        }
        [HttpGet("GetEnableplaces")]
        public async Task<IActionResult> GetEnablePlaces()
        {
            var existbookings = await _db.Places
                                    .Where(p => p.PlaceStatus == MyStatus.enabled)
                                    .Include(u => u.user)
                                    .OrderBy(p=>p.PlaceId)
                                    .ToListAsync();
            if (existbookings == null)
            {
                return NotFound("is not found");
            }
            return Ok(existbookings);
        }
        [HttpGet("GetDisabledplaces")]
        public async Task<IActionResult> GetDisabledplaces()
        {
            var existbookings = await _db.Places
                                    .Where(p => p.PlaceStatus == MyStatus.disabled)
                                    .Include(u => u.user)
                                    .OrderBy(p => p.PlaceId)
                                    .ToListAsync();
            if (existbookings == null)
            {
                return NotFound("is not found");
            }
            return Ok(existbookings);
        }
        [HttpGet("GetPendingplaces")]
        public async Task<IActionResult> GetPendingPlaces()
        {
            var existbookings = await _db.Places                                   
                                    .Where(p => p.PlaceStatus == MyStatus.pending)
                                    .Include(p =>p.Listimage.OrderBy(i => i.ImageId))
                                    .Include(p=>p.user)
                                    .OrderBy(p=>p.PlaceId)
                                    .ToListAsync();
            if (existbookings == null)
            {
                return NotFound("is not found");
            }
            return Ok(existbookings);
        }
        [HttpGet("EnablePlace{placeId}")]
        public async Task<IActionResult> EnablePlace(int placeId)
        {
            if (placeId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existplace = await _db.Places
                                    .Where(p => p.PlaceId == placeId)
                                    .Where(p => p.PlaceStatus == MyStatus.pending)
                                    .FirstOrDefaultAsync();
            if (existplace == null)
            {
                return NotFound("is not found or enabled");
            }
            existplace.PlaceStatus = MyStatus.enabled;
            await SentNotificationAsync(existplace.OwnerId, "the admin accept your chalet and it added to your chalets");
            await _db.SaveChangesAsync();
            return Ok("place is enableing");
        }
        [HttpGet("DisabledPlace{placeId}")]
        public async Task<IActionResult> DisabledPlace(int placeId)
        {
            if (placeId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existplace = await _db.Places
                                    .Where(p => p.PlaceId == placeId)
                                    .Where(p => p.PlaceStatus == MyStatus.pending)
                                    .FirstOrDefaultAsync();
            if (existplace == null)
            {
                return NotFound("is not found");
            }
            existplace.PlaceStatus = MyStatus.disabled;
            await SentNotificationAsync(existplace.OwnerId, "the admin reject your chalet");
            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpGet("GetEnableBookings")]
        public async Task<IActionResult> GetEnableBookings()
        {
            var existbookings = await _db.Bookings
                                    .Where(b => b.StatusBooking == MyStatus.enabled)
                                    .Include(u => u.user)
                                    .Include(b=>b.place)
                                    .OrderBy(b => b.BookingId)
                                    .ToListAsync();
            if (existbookings == null)
            {
                return NotFound("is not found");
            }
            return Ok(existbookings);
        }
        [HttpGet("GetDisabledBookings")]
        public async Task<IActionResult> GetDisabledBookings()
        {
            var existbookings = await _db.Bookings
                                    .Where(b => b.StatusBooking == MyStatus.disabled)
                                    .Include(u => u.user)
                                    .Include(b => b.place)
                                    .OrderBy(b => b.BookingId)
                                    .ToListAsync();
            if (existbookings == null)
            {
                return NotFound("is not found");
            }
            return Ok(existbookings);
        }
        [HttpGet("GetPendingBookings")]
        public async Task<IActionResult> GetPendingBookings()
        {
            var existbookings = await _db.Bookings
                                    .Where(b => b.StatusBooking == MyStatus.pending)
                                    .Include(u => u.user)
                                    .Include(b => b.place)
                                    .OrderBy(b => b.BookingId)
                                    .ToListAsync();
            if (existbookings == null)
            {
                return NotFound("is not found");
            }
            return Ok(existbookings);
        }
        private async Task SentNotificationAsync(int userid, string message)
        {
            await _db.Notifications.AddAsync(new Notification {UserId = userid, Message = message });
        }

    }
}
