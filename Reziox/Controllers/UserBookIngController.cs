
using Microsoft.AspNetCore.Mvc;
using Reziox.Model.ThePlace;
using Reziox.Model;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Model.DTO;

namespace Rezioxgithub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserBookIngController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UserBookIngController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("Check")]
        public async Task<IActionResult> FirstAddBooking(int placeId, int userId, DateOnly datebooking)
        {
            try
            {
                if (placeId == 0 || userId == 0)
                {
                    return BadRequest($" 0 id is not correct ");
                }
                if (datebooking.DayOfYear < DateTime.Today.DayOfYear)
                {
                    return BadRequest("this date in the past");
                }
                //find exist place and user
                var existplace = await _db.Places.Where(p => p.PlaceId == placeId).FirstOrDefaultAsync();
                var existuser = await _db.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
                if (existplace == null || existuser == null)
                {
                    return NotFound("User or Place not found.");
                }
                if (existuser.UserId == existplace.OwnerId)
                {
                    return BadRequest("can not booking your chalet !");
                }
                //check already user booked this place
                var existalreadybooking = await _db.Bookings
                                            .Where(b => b.PlaceId == placeId)
                                            .Where(b => b.UserId == existuser.UserId)
                                            .Where(b => b.BookingDate.DayOfYear == datebooking.DayOfYear)
                                            .Where(b => b.StatusBooking == MyStatus.confirmation || b.StatusBooking == MyStatus.pending || b.StatusBooking == MyStatus.approve)
                                            .FirstOrDefaultAsync();
                if (existalreadybooking != null && existalreadybooking.Typeshifts == MyShifts.full)
                {
                    return BadRequest("you already booked this place");
                }
                //end check already user booked this place

                //check place is working
                var daybooking = datebooking.DayOfWeek.ToString();
                if (!Enum.TryParse(daybooking.ToLower(), out MYDays day))
                {
                    return BadRequest($"invalid day :{daybooking}");
                }
                if ((existplace.WorkDays & day) != day)
                {
                    return BadRequest("the chalete is not working !");
                }
                //end check is booked or not

                //check is booked or not
                var existbooking = await _db.Bookings
                                            .Where(b => b.PlaceId == placeId)
                                            .Where(b => b.UserId != existuser.UserId)
                                            .Where(b => b.BookingDate.DayOfYear == datebooking.DayOfYear)
                                            .Where(b => b.StatusBooking == MyStatus.confirmation)
                                            .FirstOrDefaultAsync();
                if (existbooking != null && existbooking.Typeshifts == MyShifts.full)
                {
                    return Content("this palce is booking now!");
                }
                //end check is booked or not
                bool aviableNightShift = true;
                bool aviableMornningShift = true;
                if (existbooking != null)
                {
                    aviableMornningShift = existbooking.Typeshifts != MyShifts.morning;
                    aviableNightShift = existbooking.Typeshifts != MyShifts.night;
                }
                var mornningtime = $"{existplace.MorrningShift} AM - {existplace.NightShift - 1} PM";
                var nighttime = $"{existplace.NightShift} PM - {existplace.MorrningShift - 1} AM";
                return Ok(new
                {
                    Mornning = aviableMornningShift,
                    Night = aviableNightShift,
                    TimeMornning = mornningtime,
                    TimeNight = nighttime
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("Confirm")]
        public async Task<IActionResult> SecondAddBooking(int placeId, int userId, DateOnly datebooking,string bookinshift)
        {
            try
            {
                if (placeId == 0 || userId == 0)
                {
                    return BadRequest($" 0 id is not correct ");
                }
                if (datebooking.DayOfYear < DateTime.Today.DayOfYear)
                {
                    return BadRequest("this date in the past");
                }
                //find exist place and user
                var existplace = await _db.Places.Where(p => p.PlaceId == placeId).FirstOrDefaultAsync();
                var existuser = await _db.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
                if (existplace == null || existuser == null)
                {
                    return NotFound("User or Place not found.");
                }
                if (existuser.UserId == existplace.OwnerId)
                {
                    return BadRequest("can not booking your chalet !");
                }
                //check already user booked this place
                var existalreadybooking = await _db.Bookings
                                            .Where(b => b.PlaceId == placeId)
                                            .Where(b => b.UserId == existuser.UserId)
                                            .Where(b => b.BookingDate.DayOfYear == datebooking.DayOfYear)
                                            .Where(b => b.StatusBooking == MyStatus.confirmation || b.StatusBooking == MyStatus.pending||b.StatusBooking == MyStatus.approve)
                                            .FirstOrDefaultAsync();
                if (existalreadybooking != null && existalreadybooking.Typeshifts == MyShifts.full)
                {
                    return BadRequest("you already booked this place");
                }
                //end check already user booked this place
                //check place is working
                var daybooking = datebooking.DayOfWeek.ToString();
                if (!Enum.TryParse(daybooking.ToLower(), out MYDays day))
                {
                    return BadRequest($"invalid day :{daybooking}");
                }
                if ((existplace.WorkDays & day) != day)
                {
                    return BadRequest("the chalete is not working !");
                }
                //end check place is working

                if (!Enum.TryParse(bookinshift.ToLower(), out MyShifts typeshift))
                {                    
                    return BadRequest($"can not convert this enum{bookinshift}");
                }
                //start check is booked or not
                var existbooking = await _db.Bookings
                                            .Where(b => b.PlaceId == placeId)
                                            .Where(b => b.UserId != existuser.UserId)
                                            .Where(b => b.BookingDate.DayOfYear == datebooking.DayOfYear)
                                            .Where(b => b.StatusBooking == MyStatus.confirmation)
                                            .Where(b => (b.Typeshifts & typeshift) == typeshift)
                                            .FirstOrDefaultAsync();
                if (existbooking != null)
                {
                    return BadRequest("this palce is booking now!");
                }
                //end check is booked or not
                var mybooking = new Booking { PlaceId = existplace.PlaceId, UserId = existuser.UserId, BookingDate = datebooking, Typeshifts = typeshift };
                await _db.Bookings.AddAsync(mybooking);
                await SentNotificationAsync(existplace.OwnerId, "New Requset booking", $"A place {existplace.PlaceName } you own is reserved on date {datebooking}");
                await SentNotificationAsync(existuser.UserId, "Requset booking Confirmation", $"Bookingis on date {datebooking} is pending and sent successfully to owner, please waite the of owner");
                await _db.SaveChangesAsync();
                return Ok("booking is pending and sent successfully to owner, please waite the of owner");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }           
        }
        [HttpDelete("Cancel/{bookingId}")]
        public async Task<IActionResult> Cancel([FromRoute] int bookingId)
        {
            try
            {
                if (bookingId == 0)
                {
                    return BadRequest("id 0 is not crrect");
                }
                var existbooking = await _db.Bookings
                                            .Where(b => b.BookingId == bookingId)
                                            .Where(b => b.StatusBooking == MyStatus.confirmation)
                                            .Include(b => b.place)
                                            .FirstOrDefaultAsync();

                if (existbooking == null)
                {
                    return NotFound("booking not found.");
                }
                //condtion for cancle
                if ((existbooking.BookingDate.DayOfYear - DateTime.Today.DayOfYear)<3)
                {
                    return BadRequest("can not cancle condition must canceling date  at maximum 3 days before booking date ");
                }
                existbooking.StatusBooking = MyStatus.cancel;
                //add notifiacation for  owner and user
                await SentNotificationAsync(existbooking.place.OwnerId, "Cancel Confirmation", $"A booking at your place has been canceled for the date {existbooking.BookingDate}");
                await SentNotificationAsync(existbooking.UserId, "Cancel Confirmation", $"You canceled the booking on date {existbooking.BookingDate}");
                await _db.SaveChangesAsync();
                return Ok("Booking canceled successfully..");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }          
        }
        [HttpGet("Details/{bookingId}")]
        public async Task<IActionResult> Details([FromRoute] int bookingId)
        {
            try
            {
                if (bookingId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbooking = await _db.Bookings
                                             .Where(p => p.BookingId == bookingId)
                                             .Where(b => b.StatusBooking == MyStatus.confirmation)
                                             .Include(u => u.user)
                                             .Include(b => b.place)                                                                                          
                                             .FirstOrDefaultAsync();
                if (existbooking == null)
                {
                    return NotFound("is not found");
                }


                // set range time 
                var rangetime = "";
                if (existbooking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{existbooking.place.MorrningShift}AM - {existbooking.place.NightShift - 1}PM";
                }
                if (existbooking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{existbooking.place.NightShift}PM - {existbooking.place.MorrningShift - 1}AM";
                }
                if (existbooking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{existbooking.place.MorrningShift} - {existbooking.place.MorrningShift - 1}:23 hours ";
                }
                var detailsbooking = new dtoDetailsBooking
                {
                    UserId = existbooking.UserId,
                    PlaceId = existbooking.PlaceId,
                    PlaceName = existbooking.place.PlaceName,
                    PlacePhone = existbooking.place.PlacePhone,
                    BookingDate = $"{existbooking.BookingDate.DayOfWeek}-{existbooking.BookingDate}",
                    Time = rangetime,
                    Price = existbooking.place.Price,
                    Firstpayment=existbooking.place.Firstpayment,
                    City = existbooking.place.City.ToString(),
                    MaxGust = existbooking.place.Visitors,
                    UserName = existbooking.user.UserName,
                    UserPhone = existbooking.user.PhoneNumber
                };
                return Ok(detailsbooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetBookings/{userId}")]
        public async Task<IActionResult> GetBookings([FromRoute] int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbookings = await _db.Bookings
                                             .Where(b => b.UserId == userId)
                                             .Where(b => b.StatusBooking == MyStatus.confirmation)
                                             .Where(p => p.BookingDate.DayOfYear >= DateTime.UtcNow.DayOfYear)
                                             .Include(b => b.place)
                                             .ThenInclude(p => p.Listimage)
                                             .OrderBy(b => b.BookingDate)
                                             .ToListAsync();
                if (existbookings == null)
                {
                    return NotFound("is not found");
                }
                var bookings = await CreateCardBookings(existbookings);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetBookingRequsets/{userId}")]
        public async Task<IActionResult> GetBookingRequsets([FromRoute] int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbookings = await _db.Bookings
                                             .Where(b => b.UserId == userId)
                                             .Where(b => b.StatusBooking == MyStatus.pending || b.StatusBooking == MyStatus.approve)
                                             .Where(p => p.BookingDate.DayOfYear >= DateTime.UtcNow.DayOfYear)
                                             .Include(b => b.place)
                                             .ThenInclude(p => p.Listimage)
                                             .OrderBy(b => b.BookingDate)
                                             .ToListAsync();
                if (existbookings == null)
                {
                    return NotFound("is not found");
                }
                var bookings = await CreateCardBookings(existbookings);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("History/{userId}")]
        public async Task<IActionResult> History([FromRoute] int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbookings = await _db.Bookings
                                             .Where(b => b.UserId == userId)
                                             .Where(b => b.StatusBooking == MyStatus.confirmation)
                                             .Where(b => b.BookingDate.DayOfYear<DateTime.UtcNow.DayOfYear)
                                             .Include(b => b.place)
                                             .ThenInclude(p => p.Listimage)
                                             .OrderBy(b => b.BookingDate)
                                             .ToListAsync();
                if (existbookings.Count != 0)
                {
                    return NotFound("is not found");
                }
                var bookings = await CreateCardBookings(existbookings);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private async Task SentNotificationAsync(int userid,string title, string message)
        {
            await _db.Notifications.AddAsync(new Notification { UserId = userid,Title=title, Message = message });
        }
        private async Task<List<dtoCardBookingSchedule>> CreateCardBookings(List<Booking> bookings)
        {   string rangetime="hh:mm";
            var cardbookings = new List<dtoCardBookingSchedule>();
            foreach (var booking in bookings)
            {
                TimeSpan dif = booking.BookingDate.ToDateTime(
                                                   TimeOnly.MinValue.AddHours(booking.place.MorrningShift)
                                                   )- DateTime.UtcNow;

                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.NightShift - 1}PM";
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift}PM - {booking.place.MorrningShift - 1}AM";
                    dif = booking.BookingDate.ToDateTime(
                                              TimeOnly.MinValue.AddHours(booking.place.MorrningShift + booking.place.NightShift)
                                              )- DateTime.UtcNow;
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $" 23 hours from start.. ";
                }
                cardbookings.Add(new dtoCardBookingSchedule
                {
                    BookingId = booking.BookingId,
                    BaseImage = booking.place.Listimage.Count != 0 ? booking.place.Listimage.OrderBy(i => i.ImageId).FirstOrDefault().ImageUrl : null,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToString(),
                    Time = rangetime,
                    CountDown = $"{dif.Days} day & {dif.Hours}h : {dif.Minutes}m"

                });
            }
            return cardbookings;
        }
    }
}
