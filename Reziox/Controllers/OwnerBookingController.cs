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
    public class OwnerBookingController : ControllerBase
    {
        private readonly AppDbContext _db;
        public OwnerBookingController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet("BookingsOwner{ownerId}")]
        public async Task<IActionResult> GetBookingsForOwner(int ownerId)
        {
            if (ownerId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existbookings = await _db.Bookings
                                    .Where(p => p.place.OwnerId == ownerId)
                                    .Where(p => p.StatusBooking == MyStatus.enabled)
                                    .Where(p => p.BookingDate.DayOfYear > DateTime.Now.DayOfYear)
                                    .Include(b => b.place)
                                    .ThenInclude(p => p.Listimage.OrderBy(i => i.ImageId))
                                    .OrderBy(p => p.BookingDate)
                                    .ToListAsync();
            if (!existbookings.Any())
            {
                return NotFound("is not found");
            }
            var bookings = CardBookings(existbookings);
            return Ok(bookings);
        }
        [HttpGet("GetPendingBookings{ownerId}")]
        public async Task<IActionResult> GetPendingBookings(int ownerId)
        {
            if (ownerId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existbookings = await _db.Bookings
                                        .Where(p => p.place.OwnerId == ownerId)
                                        .Where(p => p.StatusBooking == MyStatus.pending)
                                        .Include(u => u.user)
                                        .Include(b => b.place)
                                        .ThenInclude(p=>p.Listimage.OrderBy(i => i.ImageId))
                                        .OrderBy(p => p.BookingDate)
                                        .ToListAsync();
            if(!existbookings.Any())
            {
                return NotFound("is not found");
            }
            var requstbookings =await CardRequstBookings(existbookings);
            return Ok(requstbookings);
        }
        [HttpGet("EnableBooking{bookingId}")]
        public async Task<IActionResult> EnableBooking(int bookingId)
        {
            if (bookingId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existbooking = await _db.Bookings
                                        .Where(p => p.BookingId == bookingId)
                                        .Where(p => p.StatusBooking == MyStatus.pending)
                                        .FirstOrDefaultAsync();
            if (existbooking == null)
            {
                return NotFound("is not found");
            }
            //check if find another booking in this date
            var anotherbooking = await _db.Bookings
                                          .Where(b => b.PlaceId == existbooking.PlaceId)
                                          .Where(b => b.BookingDate.DayOfYear ==existbooking.BookingDate.DayOfYear)
                                          .Where(b => b.StatusBooking == MyStatus.enabled)
                                          .Where(b => (b.Typeshifts & existbooking.Typeshifts) == existbooking.Typeshifts)
                                          .FirstOrDefaultAsync();
            if (anotherbooking != null)
            {
                await DisabledBooking(existbooking.BookingId);
            }
            // end check if find another booking in this date
            existbooking.StatusBooking = MyStatus.enabled;
            await SentNotificationAsync(existbooking.UserId, "the owner accept your booking and it added to your bookings");
            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpGet("DisabledBooking{bookingId}")]
        public async Task<IActionResult> DisabledBooking(int bookingId)
        {
            if (bookingId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existbooking = await _db.Bookings
                                        .Where(p => p.BookingId == bookingId)
                                        .Where(p => p.StatusBooking == MyStatus.pending)
                                        .FirstOrDefaultAsync();
            if (existbooking == null)
            {
                return NotFound("is not found");
            }
            existbooking.StatusBooking = MyStatus.disabled;
            await SentNotificationAsync(existbooking.UserId, "the owner reject your booking");
            await _db.SaveChangesAsync();
            return Ok();
        }
        private async Task<List<dtoCardSchedule>> CardBookings(List<Booking> bookings)
        {
            string rangetime = "hh:mm";
            var cardbookings = new List<dtoCardSchedule>();
            foreach (var booking in bookings)
            {
                TimeSpan dif = booking.BookingDate.ToDateTime(TimeOnly.MinValue) - DateTime.Now;

                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.NightShift - 1}PM";
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift}PM - {booking.place.MorrningShift - 1}AM";
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{booking.place.MorrningShift} - {booking.place.MorrningShift - 1}:23 hours ";
                }
                cardbookings.Add(new dtoCardSchedule
                {
                    BookingId = booking.BookingId,
                    BaseImage = booking.place.Listimage.Count != 0 ? booking.place.Listimage.ElementAt(0).ImageUrl : null,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToString(),
                    Time = rangetime,
                    CountDown = $"{dif.Days} day & {dif.Hours} hour"

                });
            }
            return cardbookings;
        }
        private async Task<List<dtoRequsetBooking>> CardRequstBookings(List<Booking> bookings)
        {
            string rangetime = "hh:mm";
            var cardbookings = new List<dtoRequsetBooking>();
            foreach (var booking in bookings)
            {
                
                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.NightShift - 1}PM";
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift}PM - {booking.place.MorrningShift - 1}AM";
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{booking.place.MorrningShift} - {booking.place.MorrningShift - 1}  (23 hours) ";
                }
                cardbookings.Add(new dtoRequsetBooking
                {
                    BookingId = booking.BookingId,
                    UserId=booking.UserId,
                    UserName=booking.user.UserName,
                    BaseImage = booking.place.Listimage.Count!=0? booking.place.Listimage.ElementAt(0).ImageUrl:null,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToString(),
                    Time = rangetime
                });
            }
            return cardbookings;
        }
        private async Task SentNotificationAsync(int userid, string message)
        {
            await _db.Notifications.AddAsync(new Notification { UserId = userid, Message = message });
        }

    }
}
