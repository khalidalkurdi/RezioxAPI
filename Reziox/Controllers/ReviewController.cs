
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;
using System.Linq;

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
            if(!ModelState.IsValid)
            {
                return BadRequest("not correct");
            }
            // Check if User and Place exist
            
            var existuser = await _db.Users.Where(u => u.UserId == userreview.UserId).FirstOrDefaultAsync();
            var existplace = await _db.Places.Where(p => p.PlaceId == userreview.PlaceId)
                                             .Where(p=>p.PlaceStatus==MyStatus.enabled)
                                             .FirstOrDefaultAsync();
            if (existuser == null || existplace == null)
            {
                return NotFound("user or place not found");
            }
            var existreview= await _db.Reviews
                                       .Where(b => b.UserId == userreview.UserId)
                                       .Where(b => b.PlaceId == userreview.PlaceId)
                                       .FirstOrDefaultAsync();
            if(existreview != null)
            {
                return Content("can not review this place, you review this already !");
            }
            var existbokking = await _db.Bookings
                                        .Where(b => b.UserId == userreview.UserId )
                                        .Where(b => b.PlaceId ==userreview.PlaceId )
                                        .Where(b=>b.StatusBooking==MyStatus.enabled)
                                        .FirstOrDefaultAsync();

            if(existbokking == null || existbokking.BookingDate.DayOfYear < DateTime.Today.DayOfYear)
            {
                return BadRequest("can not review this place,you must try the service and try review later !");
            }

            var review = new Review { PlaceId = userreview.PlaceId, UserId = userreview.UserId, Rating = userreview.Rating ,Comment=userreview.Comment};
            existplace.ListReviews.Add(review);
            await _db.SaveChangesAsync();
            return Ok("review sent successfuly");
        }

    }
}
