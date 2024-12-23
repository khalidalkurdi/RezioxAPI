using DataAccess.PublicClasses;
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
        private readonly INotificationService _notification;
        public OwnerBookingController(AppDbContext db,INotificationService notification)
        {
            _db = db;
            _notification = notification;
        }
        [HttpGet("GetBookings/{ownerId}")]
        public async Task<IActionResult> GetBookings([FromRoute]int ownerId)
        {
            try
            {
                if (ownerId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbookings = await _db.Bookings
                                            .Where(p => p.place.OwnerId == ownerId)
                                            .Where(p => p.StatusBooking == MyStatus.confirmation)
                                            .Where(p => p.BookingDate.DayOfYear >= DateTime.UtcNow.AddHours(3).DayOfYear)
                                            .Include(b => b.place)
                                            .ThenInclude(p => p.Listimage)
                                            .OrderBy(p => p.BookingDate)
                                            .ToListAsync();
                if (existbookings.Count == 0)
                {
                    return Ok(existbookings);
                }
                var bookings = CreateCardBookings(existbookings).Result;
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetPendings/{ownerId}")]
        public async Task<IActionResult> GetPendings([FromRoute] int ownerId)
        {
            try
            {
                if (ownerId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbookings = await _db.Bookings
                                            .Where(b => b.place.OwnerId == ownerId)
                                            .Where(b => b.StatusBooking == MyStatus.pending || b.StatusBooking == MyStatus.approve)
                                            .Where(b=>b.BookingDate.DayOfYear>=DateTime.UtcNow.AddHours(3).DayOfYear)
                                            .Include(u => u.user)
                                            .Include(b => b.place)
                                            .ThenInclude(p=>p.Listimage)
                                            .OrderBy(p => p.BookingDate)
                                            .ToListAsync();
                if(existbookings.Count == 0)
                {
                    return Ok(existbookings);
                }
                var requstbookings = await CreateCardRequst(existbookings);
                return Ok(requstbookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("PaymentConfirmation/{bookingId}")]
        public async Task<IActionResult> PaymentConfirmation([FromRoute] int bookingId)
        {
            try
            {
                if (bookingId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbooking = await _db.Bookings
                                            .Where(p => p.BookingId == bookingId)
                                            .Where(p => p.StatusBooking == MyStatus.approve)
                                            .FirstOrDefaultAsync();
                if (existbooking == null)
                {
                    return NotFound("is not found");
                }
                
                //check if find another booking in this date
                var booked = await _db.Bookings
                                              .Where(b => b.PlaceId == existbooking.PlaceId)
                                              .Where(b => b.UserId == existbooking.UserId)
                                              .Where(b => b.BookingDate.DayOfYear == existbooking.BookingDate.DayOfYear)
                                              .Where(b => b.StatusBooking == MyStatus.confirmation)
                                              .Where(b => (b.Typeshifts & existbooking.Typeshifts) == existbooking.Typeshifts)
                                              .FirstOrDefaultAsync();
                if (booked != null)
                {
                    await Reject(existbooking.BookingId);
                    return BadRequest(" already booked");
                }
                var anotherpendingbookings = await _db.Bookings
                                                      .Where(b => b.PlaceId == existbooking.PlaceId)
                                                      .Where(b => b.UserId != existbooking.UserId)
                                                      .Where(b => b.BookingDate.DayOfYear == existbooking.BookingDate.DayOfYear)
                                                      .Where(b => b.StatusBooking == MyStatus.approve || b.StatusBooking== MyStatus.pending)
                                                      .Where(b => (b.Typeshifts & existbooking.Typeshifts) == existbooking.Typeshifts)
                                                      .ToListAsync();
                if (anotherpendingbookings.Count != 0)
                {
                    foreach (var booking in anotherpendingbookings)
                    {
                        await Reject(booking.BookingId);       
                    }                    
                }
                // end check if find another booking in this date
                existbooking.StatusBooking = MyStatus.confirmation;
                await _notification.SentAsync(existbooking.UserId, "Payment Confirmation", "The chalet owner accept your booking and it added to your bookings schedule");
                await _db.SaveChangesAsync();
                return Ok("Confirm booking successfuly");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("Approve/{bookingId}")]
        public async Task<IActionResult> Approve([FromRoute] int bookingId)
        {
            try
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
                var approvebooking = await _db.Bookings
                                              .Where(b => b.PlaceId == existbooking.PlaceId)
                                              .Where(b => b.BookingDate.DayOfYear ==existbooking.BookingDate.DayOfYear)
                                              .Where(b => b.StatusBooking == MyStatus.confirmation)
                                              .Where(b => (b.Typeshifts & existbooking.Typeshifts) == existbooking.Typeshifts)
                                              .FirstOrDefaultAsync();                
                if (approvebooking != null)
                {
                    await Reject(existbooking.BookingId);
                    return BadRequest("already booking");
                }                
                // end check if find another booking in this date
                existbooking.StatusBooking = MyStatus.approve;
                await _notification.SentAsync(existbooking.UserId, "Acceptance Confirmation", "The chalet owner has accepted the reservation and is waiting for the first payment to confirm the reservation payment.");
                await _db.SaveChangesAsync();
                return Ok("Approve booking successfuly");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("Reject/{bookingId}")]
        public async Task<IActionResult> Reject([FromRoute] int bookingId)
        {
            try
            {
                if (bookingId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbooking = await _db.Bookings
                                            .Where(p => p.BookingId == bookingId)
                                            .Where(p => p.StatusBooking == MyStatus.pending || p.StatusBooking == MyStatus.approve)
                                            .FirstOrDefaultAsync();
                if (existbooking == null)
                {
                    return NotFound("is not found");
                }
                existbooking.StatusBooking = MyStatus.reject;
                await _notification.SentAsync(existbooking.UserId, "Rejection Confirmation", "The chalet owner reject the booking");
                await _db.SaveChangesAsync();
                return Ok("reject booking successfuly");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private async Task<List<dtoCardBookingSchedule>> CreateCardBookings(List<Booking> bookings)
        {
            string rangetime = "";
            var cardbookings = new List<dtoCardBookingSchedule>();
            foreach (var booking in bookings)
            {
                TimeSpan dif= booking.BookingDate - DateTime.UtcNow.AddHours(3); 

                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.NightShift - 1}PM";
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift}PM - {booking.place.MorrningShift - 1}AM";
                    dif = booking.BookingDate - DateTime.UtcNow.AddHours(3);
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $" 23 hours from start.. ";
                    dif = booking.BookingDate - DateTime.UtcNow.AddHours(3);
                }
                cardbookings.Add(new dtoCardBookingSchedule
                {
                    BookingId = booking.BookingId,
                    BaseImage = booking.place.Listimage.Count != 0 ? booking.place.Listimage.OrderBy(i => i.ImageId).FirstOrDefault().ImageUrl : null,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToString(),
                    Time = rangetime,
                    CountDown = $"{dif.Days} day & {dif.Hours} hour"

                });
            }
            return cardbookings;
        }
        private async Task<List<dtoCardRequsetOwner>> CreateCardRequst(List<Booking> bookings)
        {
            string rangetime = "";
            var cardbookings = new List<dtoCardRequsetOwner>();
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
                cardbookings.Add(new dtoCardRequsetOwner
                {
                    BookingId = booking.BookingId,
                    UserId=booking.UserId,
                    UserName=booking.user.UserName,
                    BaseImage = booking.user.UserImage,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToString(),
                    Time = rangetime
                });
            }
            return cardbookings;
        }
    }
}
