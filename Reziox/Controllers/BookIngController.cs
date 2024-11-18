using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reziox.Model.ThePlace;
using Reziox.Model;
using Reziox.DataAccess;
using Microsoft.EntityFrameworkCore;

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
                return BadRequest(daybooking);
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
            if (existbooking.Typeshifts==MyShifts.full)
            {
                return BadRequest("this palce is booking now!");
            }
            //end check is booked or not
            
            bool mornningshift= existbooking.Typeshifts==MyShifts.morning?true:false;
            bool nightshift= existbooking.Typeshifts==MyShifts.night?true:false;   
            return Ok( new{ Mornning =mornningshift , Night =nightshift, Full=false }) ;
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
                    return BadRequest(daybooking);
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

            var notificationOwner = new Notification
            {
                UserId = existplace.OwnerId,
                Message = $"A place you own is reserved on date{datebooking}",
                CreatedAt = DateTime.Now
            };
            _db.Notifications.Add(notificationOwner);
            var notificationUser = new Notification
            {
                UserId = existuser.UserId,
                Message = $"Your reservation has been successfully received on date {datebooking}",
                CreatedAt = DateTime.Now
            };
            await _db.Notifications.AddAsync(notificationUser);
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
            if (existbooking.BookingDate.DayOfYear == datecancel.DayOfYear)
            {
                return BadRequest("can not cancle");
            }
            _db.Bookings.Remove(existbooking);
            //add notifiacation for  owner and user
            var notificationOwner = new Notification
            {
                UserId = existbooking.place.OwnerId,
                Message = $"A booking at your place has been canceled for the date {existbooking.BookingDate}",
                CreatedAt = DateTime.Now
            };
            _db.Notifications.Add(notificationOwner);
            var notificationUser = new Notification
            {
                UserId = existbooking.UserId,
                Message = $"You canceled the booking on date {existbooking.BookingDate}",
                CreatedAt = DateTime.Now
            };

            _db.Notifications.Add(notificationUser);
            await _db.SaveChangesAsync();
            return Ok("Booking canceled successfully.");
        }
        [HttpGet("BookingsUser{userId}")]
        public async Task<IActionResult> GetBookingsForUser(int userId)
        {
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
            var bookings = await _db.Bookings
                .Where(p => p.place.OwnerId == OwnerId)
                .Include(u => u.user.UserName)
                .Include(b => b.place.PlaceName)
                .OrderDescending()
                .ToListAsync();
            if (bookings == null)
            {
                return NotFound("is not found");
            }
            return Ok(bookings);
        }
    }
}
