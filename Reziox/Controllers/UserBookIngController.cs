using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reziox.Model.ThePlace;
using Reziox.Model;

using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model.TheUsers;
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

        [HttpPost("CheckBooking")]
        public async Task<IActionResult> FirstAddBooking(int placeId, int userId, DateOnly datebooking)
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
                .Where(b => b.BookingDate.DayOfYear == datebooking.DayOfYear)
                .Where(b => b.StatusBooking==MyStatus.enabled)
                .FirstOrDefaultAsync();
            if (existbooking!=null && existbooking.Typeshifts==MyShifts.full)
            {
                return Content("this palce is booking now!");
            }
            //end check is booked or not

            bool fullshift = existbooking == null;
            bool aviablenightshift=true;
            bool aviablemornningshift=true;
            if (existbooking != null)
            {
                aviablemornningshift = existbooking.Typeshifts != MyShifts.morning;
                aviablenightshift= existbooking.Typeshifts!=MyShifts.night;                
            }
            var mornningtime = $"{existplace.MorrningShift} AM - {existplace.NightShift-1} PM";
            var nighttime = $"{existplace.NightShift} PM - {existplace.MorrningShift-1} AM";
            return Ok(new
            {
                Mornning = aviablemornningshift,
                Night = aviablenightshift,
                TimeMornning = mornningtime,
                TimeNight = nighttime

            });

        }

        [HttpPost("ConfirmBooking")]
        public async Task<IActionResult> SecondAddBooking(int placeId, int userId, DateOnly datebooking,string bookinshift)
        {
            if(placeId == 0 || userId == 0  )
            {
                return BadRequest($" 0 id is not correct ");
            }
            if (datebooking.DayOfYear<DateTime.Today.DayOfYear)
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
            //check place is working
            var daybooking = datebooking.DayOfWeek.ToString();
            if (!Enum.TryParse(daybooking.ToLower(), out MYDays day))
            {
                    return BadRequest($"invalid day :{daybooking}");
            }
            if ((existplace.WorkDays & day)!= day)
            {
                return BadRequest("the chalete is not working !");
            }
            //end check place is working

            if (!Enum.TryParse(bookinshift.ToLower(),out MyShifts typeshift))
            {
                if (!string.IsNullOrEmpty(bookinshift))
                    return BadRequest($"can not convert this enum{ bookinshift}");
            }
            //start check is booked or not
            var existbooking =await _db.Bookings
                .Where(b => b.PlaceId == placeId)
                .Where(b=> b.BookingDate.DayOfYear == datebooking.DayOfYear)
                .Where(b => b.StatusBooking == MyStatus.enabled)
                .Where(b => (b.Typeshifts& typeshift) == typeshift)
                .FirstOrDefaultAsync();
            if (existbooking != null)
            {
                return BadRequest("this palce is booking now!");
            }
            //end check is booked or not
            var mybooking = new Booking { PlaceId = existplace.PlaceId, UserId = existuser.UserId, BookingDate =datebooking,Typeshifts= typeshift  };
            await _db.Bookings.AddAsync(mybooking);
            await SentNotificationAsync(existplace.OwnerId, $"A place you own is reserved on date{datebooking}");          
            await _db.SaveChangesAsync();
            return Ok("booking is pending and sent successfully to owner, please waite the of owner");
        }

        [HttpDelete("Cancel")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            if (bookingId == 0)
            {
                return BadRequest("id 0 is not crrect");
            }
            var existbooking = await _db.Bookings
                                        .Where(b=>b.BookingId==bookingId)
                                        .Where(b => b.StatusBooking == MyStatus.enabled)
                                        .Include(b => b.place)
                                        .FirstOrDefaultAsync();

            if (existbooking == null)
            {
                return NotFound("booking not found.");
            }
            //condtion for cancle
            if (existbooking.BookingDate.DayOfYear <= DateTime.Today.DayOfYear+2)
            {
                return BadRequest("can not cancle condition must canceling date  at maximum 3 days before booking date ");
            }
            existbooking.StatusBooking = MyStatus.cancel;
            //add notifiacation for  owner and user
            await SentNotificationAsync(existbooking.place.OwnerId, $"A booking at your place has been canceled for the date {existbooking.BookingDate}");
            await SentNotificationAsync(existbooking.UserId, $"You canceled the booking on date {existbooking.BookingDate}");
            await _db.SaveChangesAsync();
            return Ok("Booking canceled successfully..");
        }
        [HttpGet("DetailsBooking{bookingId}")]
        public async Task<IActionResult> GetDetailsBooking(int bookingId)
        {
            if (bookingId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existbooking = await _db.Bookings
                                         .Where(p => p.BookingId == bookingId)
                                         .Where(b => b.StatusBooking == MyStatus.enabled)
                                         .Include(u => u.user)
                                         .Include(b => b.place)
                                         .ThenInclude(p=>p.Listimage.OrderBy(i => i.ImageId))
                                         .Include(p=>p.place.user)
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
                OwnerPhone = existbooking.place.user.PhoneNumber,
                BookingDate = $"{existbooking.BookingDate.DayOfWeek}-{existbooking.BookingDate}",
                Time = rangetime,
                Price = existbooking.place.Price,
                City = existbooking.place.City.ToString(),
                MaxGust = existbooking.place.Visitors,
                UserName = existbooking.user.UserName,
                UserPhone = existbooking.user.PhoneNumber
            };
            return Ok(detailsbooking);
        }
        [HttpGet("BookingsUser{userId}")]
        public async Task<IActionResult> GetBookingsForUser(int userId)
        {
            if(userId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existbookings = await _db.Bookings
                                         .Where(b => b.UserId == userId)
                                         .Where(b => b.StatusBooking == MyStatus.enabled)
                                         .Where(p => p.BookingDate.DayOfYear >= DateTime.Now.DayOfYear)
                                         .Include(b => b.place)
                                         .ThenInclude(p => p.Listimage.OrderBy(i => i.ImageId))
                                         .OrderBy(b => b.BookingDate)
                                         .ToListAsync();
            if (existbookings == null)
            {
                return NotFound("is not found");
            }
            var bookings = CreateCardBookings(existbookings).Result;
            return Ok(bookings);
        }
        
        private async Task SentNotificationAsync(int userid, string message)
        {
            await _db.Notifications.AddAsync(new Notification { UserId = userid, Message = message });
        }
        private async Task<List<dtoCardBookingSchedule>> CreateCardBookings(List<Booking> bookings)
        {   string rangetime="hh:mm";
            var cardbookings = new List<dtoCardBookingSchedule>();
            foreach (var booking in bookings)
            {
                TimeSpan dif=booking.BookingDate.ToDateTime(TimeOnly.MinValue) - DateTime.Now;
                
                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.NightShift-1}PM";
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift}PM - {booking.place.MorrningShift-1}AM";
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{booking.place.MorrningShift} - {booking.place.MorrningShift-1}:23 hours ";
                }
                cardbookings.Add(new dtoCardBookingSchedule
                {
                    BookingId = booking.BookingId,
                    BaseImage = booking.place.Listimage.Count != 0 ? booking.place.Listimage.ElementAt(0).ImageUrl : null,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToString(),
                    Time = rangetime,
                    CountDown=$"{dif.Days} day & {dif.Hours} hour"

                }) ;
            }
            return cardbookings;
        }
        
    }
}
