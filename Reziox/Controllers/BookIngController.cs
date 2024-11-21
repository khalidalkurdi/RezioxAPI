using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reziox.Model.ThePlace;
using Reziox.Model;

using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model.TheUsers;

namespace Rezioxgithub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookIngController : ControllerBase
    {
        private readonly AppDbContext _db;
        public BookIngController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("checkdate")]
        public async Task<IActionResult> FirstAddBooking(int placeId, int userId, DateTime datebooking)
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
            var existplace = await _db.Places.FirstOrDefaultAsync(p => p.PlaceId == placeId);
            var existuser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
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
            if (!Enum.TryParse(daybooking, out MYDays day))
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
                .FirstOrDefaultAsync();
            if (existbooking!=null && existbooking.Typeshifts==MyShifts.full)
            {
                return BadRequest("this palce is booking now!");
            }
            //end check is booked or not

            bool fullshift = existbooking == null;
            bool mornningshift = existbooking.Typeshifts != MyShifts.morning;
            bool nightshift= existbooking.Typeshifts!=MyShifts.night;
 
            var mornningtime = $"{existplace.MorrningShift} AM - {existplace.NightShift} PM";
            var nighttime = $"{existplace.NightShift } PM - {existplace.MorrningShift} AM";
            var fulltime = $"24 hours after start({existplace.NightShift } PM or {existplace.MorrningShift} AM)";
            return Ok( new{ 
                Mornning =mornningshift ,
                Night =nightshift,
                Full=fullshift,
                TimeMornning=mornningtime,
                TimeNight=nighttime,
                TimeFull=fulltime
            }) ;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddBooking(int placeId, int userId, DateTime datebooking,string bookinshift)
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
            var existplace = await _db.Places.FirstOrDefaultAsync(p => p.PlaceId == placeId);
            var existuser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
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
                .Where(b => (b.Typeshifts& typeshift) == typeshift)
                .FirstOrDefaultAsync();
            if (existbooking != null)
            {
                return BadRequest("this palce is booking now!");
            }
            //end check is booked or not
            var mybooking = new Booking { PlaceId = existplace.PlaceId, UserId = existuser.UserId, BookingDate =datebooking,Typeshifts= typeshift  };
            await _db.Bookings.AddAsync(mybooking);
            await SentNotificationAsync(existuser.UserId, $"Your reservation has been successfully received on date {datebooking}");
            await SentNotificationAsync(existplace.OwnerId, $"A place you own is reserved on date{datebooking}");
           
            await _db.SaveChangesAsync();
            return Ok("Booking added successfully");
        }

        [HttpDelete("Cancel")]
        public async Task<IActionResult> CancelBooking(int bookingId, DateOnly datecancel)
        {
            if (bookingId == 0)
            {
                return BadRequest("Id 0 is not crrect");
            }
            var existbooking = await _db.Bookings
                .Where(b=>b.BookingId==bookingId)
                .FirstOrDefaultAsync();

            if (existbooking == null)
            {
                return NotFound("Booking not found.");
            }
            //condtion for cancle
            if (existbooking.BookingDate.DayOfYear == datecancel.DayOfYear ||
                existbooking.BookingDate.DayOfYear-3==datecancel.DayOfYear)
            {
                return BadRequest("can not cancle");
            }
            _db.Bookings.Remove(existbooking);
            //add notifiacation for  owner and user
            await SentNotificationAsync(existbooking.place.OwnerId, $"A booking at your place has been canceled for the date {existbooking.BookingDate}");
            await SentNotificationAsync(existbooking.UserId, $"You canceled the booking on date {existbooking.BookingDate}");
            await _db.SaveChangesAsync();
            return Ok("Booking canceled successfully..");
        }
        [HttpGet("BookingsUser{userId}")]
        public async Task<IActionResult> GetBookingsForUser(int userId)
        {
            if(userId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var bookings = await _db.Bookings
                                    .Where(b => b.UserId == userId)
                                    .Include(b => b.place)
                                    .OrderDescending()
                                    .ToListAsync();
            if (bookings == null)
            {
                return NotFound("is not found");
            }
            return Ok(bookings);
        }
        [HttpGet("BookingsOwner{OwnerId}")]
        public async Task<IActionResult> GetBookingsForOwner(int OwnerId)
        {
            if (OwnerId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var bookings = await _db.Bookings
                                    .Where(p => p.place.OwnerId == OwnerId)
                                    .Include(u => u.user)
                                    .Include(b => b.place)
                                    .OrderDescending()
                                    .ToListAsync();
            if (bookings == null)
            {
                return NotFound("is not found");
            }
            return Ok(bookings);
        }
        private async Task SentNotificationAsync(int userid, string message)
        {
            await _db.Notifications.AddAsync(new Notification { Id = userid, Message = message });
        }

    }
}
