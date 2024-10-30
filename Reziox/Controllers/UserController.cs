using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model;

namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UserController(AppDbContext db)
        {

            _db = db;

        }
        [HttpPost]
        public async Task<IActionResult> AddToFavorites(int userId, int placeId)
        {
            var favorite = new Favorite { UserId = userId, PlaceId = placeId };
            _db.Favorites.Add(favorite);
            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete]
        public async Task<IActionResult> RemoveFromFavorites(int userId, int placeId)
        {
            var favorite = await _db.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PlaceId == placeId);

            if (favorite == null)
            {
                return NotFound("Favorite not found.");
            }

            _db.Favorites.Remove(favorite);
            await _db.SaveChangesAsync();

            return Ok("Removed from favorites.");
        }
    }
}
