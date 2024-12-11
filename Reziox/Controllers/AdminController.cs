using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;

namespace RezioxAPIs.Controllers
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
            var existPlaces = await _db.Places
                                    .Where(p => p.PlaceStatus == MyStatus.enabled)
                                    .Include(u => u.user)
                                    .OrderBy(p=>p.PlaceId)
                                    .ToListAsync();
            if (existPlaces == null)
            {
                return NotFound("is not found");
            }
            var cardsPlaces =await CreateCardPlaces(existPlaces);
            return Ok(cardsPlaces);
        }
        [HttpGet("GetDisabledplaces")]
        public async Task<IActionResult> GetDisabledplaces()
        {
            var existPlaces = await _db.Places
                                    .Where(p => p.PlaceStatus == MyStatus.disabled)
                                    .Include(u => u.user)
                                    .OrderBy(p => p.PlaceId)
                                    .ToListAsync();
            if (existPlaces == null)
            {
                return NotFound("is not found");
            }
            var cardsPlaces =await CreateCardPlaces(existPlaces);
            return Ok(cardsPlaces);
        }
        [HttpGet("GetPendingplaces")]
        public async Task<IActionResult> GetPendingPlaces()
        {
            var existPlaces = await _db.Places                                   
                                        .Where(p => p.PlaceStatus == MyStatus.pending)
                                        .Include(p =>p.Listimage)
                                        .Include(p=>p.user)
                                        .OrderBy(p=>p.PlaceId)
                                        .ToListAsync();
            if (existPlaces == null)
            {
                return NotFound("is not found");
            }
            var cardsPlaces = await CreateCardPlaces(existPlaces);
            return Ok(cardsPlaces);
        }
        [HttpGet("EnablePlace/{placeId}")]
        public async Task<IActionResult> EnablePlace(int placeId)
        {
            if (placeId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existplace = await _db.Places
                                    .Where(p => p.PlaceId == placeId)
                                    .Where(p => p.PlaceStatus == MyStatus.pending|| p.PlaceStatus == MyStatus.disabled)
                                    .FirstOrDefaultAsync();
            if (existplace == null)
            {
                return NotFound("is not found or enabled");
            }
            existplace.PlaceStatus = MyStatus.enabled;
            await SentNotificationAsync(existplace.OwnerId, "the admin accept your chalet and it added to your chalets");
            await _db.SaveChangesAsync();
            return Ok("place is enabled");
        }
        [HttpGet("DisabledPlace/{placeId}")]
        public async Task<IActionResult> DisablePlace(int placeId)
        {
            if (placeId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existplace = await _db.Places
                                    .Where(p => p.PlaceId == placeId)
                                    .Where(p => p.PlaceStatus == MyStatus.pending || p.PlaceStatus == MyStatus.enabled)
                                    .FirstOrDefaultAsync();
            if (existplace == null)
            {
                return NotFound("is not found or already disabled");
            }
            existplace.PlaceStatus = MyStatus.disabled;
            await SentNotificationAsync(existplace.OwnerId, "the admin reject your chalet");
            await _db.SaveChangesAsync();
            return Ok("place is disabled");
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
        private async Task<List<dtoCardPlace>> CreateCardPlaces(List<Place> places)
        {

            var cardplaces = new List<dtoCardPlace>();
            foreach (var place in places)
            {
                cardplaces.Add(new dtoCardPlace
                {
                    PlaceId = place.PlaceId,
                    PlaceName = place.PlaceName,
                    Price = place.Price,
                    City = place.City.ToString(),
                    Visitors = place.Visitors,
                    Rating = place.Rating,
                    BaseImage = place.Listimage.Count != 0 ? place.Listimage.OrderBy(i => i.ImageId).FirstOrDefault().ImageUrl : null
                });
            }
            return cardplaces;
        }

    }
}
