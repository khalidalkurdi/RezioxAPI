using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;


namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _db;
 
        public ReviewController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] dtoReview userreview)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("not correct");
                }
                // Check if User and Place exist
            
                var existuser = await _db.Users.AsNoTracking()
                                                .Where(u => u.UserId == userreview.UserId)
                                                .FirstOrDefaultAsync();
                var existplace = await _db.Places.AsNoTracking()
                                                 .Where(p => p.PlaceId == userreview.PlaceId)
                                                 .Where(p=>p.PlaceStatus==MyStatus.approve)
                                                 .FirstOrDefaultAsync();
                if (existuser == null || existplace == null)
                {
                    return NotFound("user or place not found");
                }
                var existreview= await _db.Reviews.AsNoTracking()
                                                   .Where(b => b.UserId == userreview.UserId)
                                                   .Where(b => b.PlaceId == userreview.PlaceId)
                                                   .FirstOrDefaultAsync();
                if(existreview != null)
                {
                    return Content("can not review this place, already you review this chalet  !");
                }
                var existbokking = await _db.Bookings.AsNoTracking()
                                                    .Where(b => b.UserId == userreview.UserId )
                                                    .Where(b => b.PlaceId ==userreview.PlaceId )
                                                    .Where(b=>b.StatusBooking==MyStatus.confirmation)
                                                    .FirstOrDefaultAsync();

                if(existbokking == null || existbokking.BookingDate.DayOfYear < DateTime.Today.DayOfYear)
                {
                    return BadRequest("can not review this place, you must try the chalet and try review later !");
                }

                var review = new Review { PlaceId = userreview.PlaceId, UserId = userreview.UserId, Rating = userreview.Rating ,Comment=userreview.Comment};
                await _db.Reviews.AddAsync(review);
                await _db.SaveChangesAsync();
                return Ok("review sent successfuly");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
