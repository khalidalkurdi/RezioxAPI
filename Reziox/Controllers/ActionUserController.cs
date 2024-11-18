
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;

namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionUserController : ControllerBase
    {
        private readonly AppDbContext _db; 
        public ActionUserController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet("GetNotifications{userId}")]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            if (userId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                //order form new to old
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            if (notifications == null)
            {
                return NotFound("no notifications found for this user.");
            }

            return Ok(notifications);
        }

        [HttpGet("GetFavorites{userId}")]
        public async Task<IActionResult> GetFavorites(int userId)
        {
            if (userId == 0)
            {
                return BadRequest("0 id is not correct !");
            }

            var existuser = await _db.Users.FindAsync(userId);
            if (existuser==null)
            {
                return NotFound("no favorites found for this user");
            }
            
            var favorites = await _db.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.place)
                .ToListAsync();
            
            return Ok(favorites);
        }
        [HttpPost("AddFavorites{userId}")]
        public async Task<IActionResult> AddToFavorites(int userId, int placeId)
        {
            if (userId==0|| placeId==0) 
            {
                return BadRequest("0 id is not correct");
            }
            var existuser = await _db.Users.FindAsync(userId);
            if (existuser == null)
            {
                return NotFound($"{userId} is not exist");
            }
            var existplace = await _db.Places.FindAsync(placeId);
            if (existplace == null)
            {
                return NotFound($"{placeId} is not exist");
            }
            var fav=await _db.Favorites.Where(u=>u.UserId==userId)
                                       .Where(p=>p.PlaceId==placeId)
                                       .FirstOrDefaultAsync();
            if (fav != null)
            {
                return BadRequest("already added");
            }
            var favorite = new Favorite { UserId = userId, PlaceId = placeId };

            await _db.Favorites.AddAsync(favorite);
            await _db.SaveChangesAsync();
            return Ok(existuser.Myfavorites.Count);
        }
        [HttpDelete("RemoveFavorites{userId}")]
        public async Task<IActionResult> RemoveFromFavorites(int userId, int placeId)
        {
            if (userId == 0 || placeId == 0)
            {
                return BadRequest("0 id is not correct");
            }
            var existfavorite = await _db.Favorites
                .Where(f => f.UserId == userId)
                .Where(f =>f.PlaceId == placeId)
                .FirstOrDefaultAsync();

            if (existfavorite == null)
            {
                return NotFound("Favorite not found ");
            }
            _db.Favorites.Remove(existfavorite);
            await _db.SaveChangesAsync();

            return Ok("Removed from favorites.");
        }
        [HttpPost("AddReview{placeId}")]
        public async Task<IActionResult> AddReview(int userId, int placeId, int rating)
        {
            if(userId==0 || placeId == 0)
            {
                return BadRequest("0 id is not correct");
            }
            // Check if User and Place exist
            var existuser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            var existplace = await _db.Places.FirstOrDefaultAsync(p => p.PlaceId == placeId);
            if (existuser == null || existplace == null)
            {
                return NotFound("user or place not found");
            }
            var existbokking = await _db.Bookings
               .Where(f => f.UserId == userId)
               .Where(f => f.PlaceId == placeId)
               .FirstOrDefaultAsync();
            if(existbokking == null || existbokking.BookingDate.Date>DateTime.Today)
            {
                return BadRequest("can not review this place !");
            }
            var review = new Review { PlaceId = placeId, UserId = userId, Rating = rating};
            existplace.ListReviews.Add(review);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
