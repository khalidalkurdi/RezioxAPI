
using Microsoft.AspNetCore.Mvc;
using Reziox.Model.ThePlace;
using Reziox.Model;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Model.DTO;
using DataAccess.PublicClasses;
using Model;

namespace Rezioxgithub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserBookIngController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notification;
        public UserBookIngController(AppDbContext db,INotificationService notification)
        {
            _db = db;
            _notification = notification;            
        }
        /// <summary>
        /// take information of bookink like date
        /// </summary>
        /// <param name="dtoSelect"></param>
        /// <returns> the avaliable shifts for spesfic date </returns>
        [HttpPost("Check")]
        public async Task<IActionResult> FirstAddBooking([FromBody]dtoSelectBooking dtoSelect)
        {
            try
            {
                if (dtoSelect.placeId == 0 || dtoSelect.userId == 0)
                {
                    return BadRequest($" 0 id is not correct ");
                }                
                //find exist place and user
                var existplace = await _db.Places.AsNoTracking()
                                                .Where(p => p.PlaceId == dtoSelect.placeId)
                                                .FirstOrDefaultAsync();
                var existuser = await _db.Users.AsNoTracking()
                                                .Where(u => u.UserId == dtoSelect.userId)
                                                .FirstOrDefaultAsync();
                if (existplace == null || existuser == null)
                {
                    return NotFound("User or Place not found.");
                }
                if (existuser.UserId == existplace.OwnerId)
                {
                    return BadRequest("can not booking your chalet !");
                }
                
                //check already user booked this place
                var existAlreadyBookings = await _db.Bookings.AsNoTracking()
                                            .Where(b => b.PlaceId == dtoSelect.placeId)
                                            .Where(b => b.UserId == existuser.UserId)
                                            .Where(b => b.BookingDate.DayOfYear == dtoSelect.datebooking.DayOfYear)
                                            .Where(b => b.StatusBooking == MyStatus.confirmation || b.StatusBooking == MyStatus.pending || b.StatusBooking == MyStatus.approve)
                                            .ToListAsync();
                if (existAlreadyBookings.Count == 2 || existAlreadyBookings.Any(b => b.Typeshifts == MyShifts.full))
                {
                    return BadRequest("you already booked this place");
                }
                //end check already user booked this place
                //check if user has booking in same date
                var existBookingSamedate = await _db.Bookings.AsNoTracking()
                                            .Where(b => b.PlaceId != dtoSelect.placeId)
                                            .Where(b => b.UserId == existuser.UserId)
                                            .Where(b => b.BookingDate.DayOfYear == dtoSelect.datebooking.DayOfYear)
                                            .Where(b => b.StatusBooking == MyStatus.confirmation)
                                            .FirstOrDefaultAsync();
                if (existBookingSamedate != null)
                {
                    return BadRequest("you already has booking in this date");
                }
                //end check if user has booking in same date
                //check place is working 
                var daybooking = dtoSelect.datebooking.DayOfWeek.ToString();
                if (!Enum.TryParse(daybooking.ToLower(), out MYDays day))
                {
                    return BadRequest($"invalid day :{daybooking}");
                }
                if ((existplace.WorkDays & day) != day)
                {
                    return BadRequest("The chalete is not working in this day !");
                }
                //end check is booked or not

                //check is booked or not
                var existbooking = await _db.Bookings.AsNoTracking()
                                            .Where(b => b.PlaceId == dtoSelect.placeId)
                                            .Where(b => b.UserId != existuser.UserId)
                                            .Where(b => b.BookingDate.DayOfYear == dtoSelect.datebooking.DayOfYear)
                                            .Where(b => b.StatusBooking == MyStatus.confirmation)
                                            .FirstOrDefaultAsync();
                //end check is booked or not
                bool aviableNightShift = true;
                bool aviableMornningShift = true;
                if (existAlreadyBookings.Count != 0)
                {                  
                    aviableMornningShift = (existAlreadyBookings.FirstOrDefault().Typeshifts != MyShifts.morning);
                    aviableNightShift = (existAlreadyBookings.FirstOrDefault().Typeshifts != MyShifts.night);                    
                }
                // card shift
                if (existbooking != null)
                {
                    aviableMornningShift = (existbooking.Typeshifts != MyShifts.morning && existbooking.Typeshifts != MyShifts.full);
                    aviableNightShift = (existbooking.Typeshifts != MyShifts.night && existbooking.Typeshifts != MyShifts.full);
                }
                if (dtoSelect.datebooking.DayOfYear == DateTime.Today.DayOfYear && DateTime.UtcNow.AddHours(3) > DateTime.MinValue.AddHours(existplace.MorrningShift))
                {
                    aviableMornningShift = false;
                }                
                //if user send requset to book one shift and try to book another shift in same date
                var mornningtime = $"{existplace.MorrningShift} AM - {existplace.NightShift - 13} PM";
                var nighttime = $"{existplace.NightShift-12} PM - {existplace.MorrningShift - 1} AM";
                //end card shift                
                return Ok(
                            new{
                            Mornning = aviableMornningShift,
                            Night = aviableNightShift,
                            TimeMornning = mornningtime,
                            TimeNight = nighttime
                            }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        /// <summary>
        /// take information of booking
        /// </summary>
        /// <param name="dtoSelect"></param>
        /// <returns> bill of bookink before added</returns>
        [HttpPost("ReviewBooking")]
        public async Task<IActionResult> ReviewBooking([FromBody] dtoSelectBooking dtoSelect)
        {
            try
            {                                
                //find exist place and user
                var existPlace = await _db.Places.AsNoTracking()
                                                .Where(p => p.PlaceId == dtoSelect.placeId)
                                                .FirstOrDefaultAsync();
                var existUser = await _db.Users.AsNoTracking()
                                                .Where(u => u.UserId == dtoSelect.userId)
                                                .FirstOrDefaultAsync();
                if (existPlace == null || existUser == null)
                {
                    return NotFound("User or Place not found.");
                }               
                if (!Enum.TryParse(dtoSelect.bookinshift.ToLower(), out MyShifts typeShift))
                {
                    return BadRequest($"can not convert this enum{dtoSelect.bookinshift}");
                }                                             
                // card of details booking before confirme booking
                var rangetime = "";
                if (typeShift == MyShifts.morning)
                {
                    rangetime = $"{existPlace.MorrningShift} AM - {existPlace.NightShift - 13} PM";
                }
                if (typeShift == MyShifts.night)
                {
                    rangetime = $"{existPlace.NightShift-12} PM - {existPlace.MorrningShift - 1} AM";
                }
                if (typeShift == MyShifts.full)
                {
                    rangetime = $"{existPlace.MorrningShift} AM - {existPlace.MorrningShift - 1} AM";
                }
                var detailsbooking = new dtoDetailsBooking
                {   
                    PlaceName = existPlace.PlaceName,
                    PlacePhone = existPlace.PlacePhone,
                    BookingDate = $"{dtoSelect.datebooking.DayOfWeek}/{dtoSelect.datebooking}",
                    Time = rangetime,
                    Price = existPlace.Price,
                    Firstpayment = existPlace.Firstpayment,
                    City = existPlace.City.ToString(),
                    MaxGust = existPlace.Visitors,
                    UserName = existUser.UserName,
                    UserPhone = existUser.PhoneNumber,
                    Shift=typeShift.ToString()
                };
                return Ok(detailsbooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        /// <summary>
        /// take information of booking for confirme it
        /// </summary>
        /// <param name="dtoSelect"></param>
        /// <returns>status code</returns>
        [HttpPost("Confirm")]
        public async Task<IActionResult> confirmBooking([FromBody]dtoSelectBooking dtoSelect)
        {
            try
            {               
                //find exist place and user
                var existPlace = await _db.Places.AsNoTracking()
                                                .Where(p => p.PlaceId == dtoSelect.placeId)
                                                .FirstOrDefaultAsync();
                var existUser = await _db.Users.AsNoTracking()
                                                .Where(u => u.UserId == dtoSelect.userId)
                                                .FirstOrDefaultAsync();
                if (existPlace == null || existUser == null)
                {
                    return NotFound("User or Place not found.");
                }
                
                var existOwner = await _db.Users.AsNoTracking()
                                                .Where(u => u.UserId == existPlace.OwnerId)
                                                .FirstOrDefaultAsync();
                //covert shift
                if (!Enum.TryParse(dtoSelect.bookinshift.ToLower(), out MyShifts typeshift))
                {                    
                    return BadRequest($"can not convert this enum{dtoSelect.bookinshift}");
                }
                //real time of start booking   >>>   calculat number of hour to date for added
                var toMakeTime = typeshift == MyShifts.night ? existPlace.NightShift : existPlace.MorrningShift;
                var mybooking = new Booking { PlaceId = existPlace.PlaceId, UserId = existUser.UserId, BookingDate = dtoSelect.datebooking.ToDateTime(TimeOnly.MinValue.AddHours(toMakeTime)), Typeshifts = typeshift };
                await _db.Bookings.AddAsync(mybooking);
                if (existOwner != null)
                {
                     await _notification.SentAsync(existOwner.DiviceToken,existOwner.UserId, "New Requset booking", $"A place {existPlace.PlaceName } you own is reserved on date {dtoSelect.datebooking}", MyScreen.OwnerRequests);
                }
                await _notification.SentAsync(existUser.DiviceToken,existUser.UserId, "Requset booking Confirmation", $"Bookingis on date {dtoSelect.datebooking} is pending and sent successfully to owner, please waite the of owner", MyScreen.UserRequsets);
                await _db.SaveChangesAsync();
                return Ok("Your Booking sent successfully to owner, please waite the of owner");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }           
        }
        /// <summary>
        /// take booking id for cancel it
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns> status code</returns>
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
                if ((existbooking.BookingDate - DateTime.UtcNow).Days<3)
                {
                    return BadRequest("can not cancle condition must canceling date  at maximum 3 days before booking date ");
                }
                existbooking.StatusBooking = MyStatus.cancel;
                //add notifiacation for  owner and user
                var existUser = await _db.Users.AsNoTracking().Where(u => u.UserId == existbooking.UserId).FirstOrDefaultAsync();
                var existOwner = await _db.Users.AsNoTracking().Where(u => u.UserId == existbooking.place.OwnerId).FirstOrDefaultAsync();
                if (existOwner != null && existUser!=null)
                {
                await _notification.SentAsync(existOwner.DiviceToken,existOwner.UserId, "Cancel Confirmation", $"A booking at your place has been canceled for the date {existbooking.BookingDate}", MyScreen.OwnerSchedule);
                await _notification.SentAsync(existUser.DiviceToken,existUser.UserId, "Cancel Confirmation", $"You canceled the booking on date {existbooking.BookingDate}", MyScreen.UserSchedule);
                }
                await _db.SaveChangesAsync();
                return Ok("Booking canceled successfully..");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }          
        }
        /// <summary>
        /// take booking id
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns>the details of booking</returns>
        [HttpGet("Details/{bookingId}")]
        public async Task<IActionResult> Details([FromRoute] int bookingId)
        {
            try
            {
                if (bookingId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbooking = await _db.Bookings.AsNoTracking()
                                             .Where(p => p.BookingId == bookingId)                                             
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
                    rangetime = $"{existbooking.place.MorrningShift}AM - {existbooking.place.NightShift - 13}PM";
                }
                if (existbooking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{existbooking.place.NightShift-12}PM - {existbooking.place.MorrningShift - 1}AM";
                }
                if (existbooking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{existbooking.place.MorrningShift}AM - {existbooking.place.MorrningShift - 1}AM";
                }
                var detailsbooking = new dtoDetailsBooking
                {
                    UserId = existbooking.UserId,
                    PlaceId = existbooking.PlaceId,
                    PlaceName = existbooking.place.PlaceName,
                    PlacePhone = existbooking.place.PlacePhone,
                    BookingDate = $"{existbooking.BookingDate.DayOfWeek}/{existbooking.BookingDate.ToString("yyyy-MM-dd")}",
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
        /// <summary>
        /// take user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>list of confirme bookings of spesfic user</returns>
        [HttpGet("GetBookings/{userId}")]
        public async Task<IActionResult> GetBookings([FromRoute] int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbookings = await _db.Bookings.AsNoTracking()
                                             .Where(b => b.UserId == userId)
                                             .Where(b => b.StatusBooking == MyStatus.confirmation)
                                             .Where(p => p.BookingDate >= DateTime.UtcNow.AddHours(3))
                                             .Include(b => b.place)
                                             .ThenInclude(p => p.Listimage)
                                             .OrderBy(b => b.BookingDate)
                                             .ToListAsync();
                if (existbookings.Count == 0)
                {
                    return Ok(existbookings);
                }
                var bookings = Card.CardBookings(existbookings);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        /// <summary>
        /// take user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>list of pending and appprove and reject bookings</returns>
        [HttpGet("GetBookingRequsets/{userId}")]
        public async Task<IActionResult> GetBookingRequsets([FromRoute] int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existBookings = await _db.Bookings.AsNoTracking()
                                             .Where(b => b.UserId == userId)
                                             .Where(b => b.StatusBooking == MyStatus.pending || b.StatusBooking == MyStatus.approve || b.StatusBooking == MyStatus.reject)
                                             .Where(p => p.BookingDate >= DateTime.UtcNow.AddHours(3))
                                             .Include(b => b.place)
                                             .ThenInclude(p => p.Listimage)
                                             .OrderByDescending(b => b.BookingId)
                                             .ToListAsync();
                if (existBookings.Count == 0)
                {
                    return Ok(existBookings);
                }
                var cardBookings = Card.CardUserRequst(existBookings);
                return Ok(cardBookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        /// <summary>
        /// take user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>list of previous bookings </returns>
        [HttpGet("History/{userId}")]
        public async Task<IActionResult> History([FromRoute] int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }
                var existbookings = await _db.Bookings.AsNoTracking()
                                             .Where(b => b.UserId == userId)
                                             .Where(b => b.StatusBooking == MyStatus.confirmation)
                                             .Where(b => b.BookingDate<DateTime.UtcNow.AddHours(3))
                                             .Include(b => b.place)
                                             .ThenInclude(p => p.Listimage)
                                             .OrderByDescending(b => b.BookingDate)
                                             .ToListAsync();
                if (existbookings.Count == 0)
                {
                    return Ok(existbookings);
                }
                var bookings = Card.CardUserHistory(existbookings);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
    }
}
