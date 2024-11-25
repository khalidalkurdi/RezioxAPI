
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;
using Rezioxgithub.Model.DTO;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Reziox.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserFavoriteController : ControllerBase
    {
        private readonly AppDbContext _db;
       
        public UserFavoriteController(AppDbContext db)
        {   
            _db = db;
           
        }

        [HttpGet("GetFavorites{userId}")]
        public async Task<IActionResult> GetFavorites(int userId)
        {
            if (userId == 0)
            {
                return BadRequest("0 id is not correct !");
            }

            var existuser = await _db.Users.FindAsync(userId);
            if (existuser == null)
            {
                return NotFound("no favorites found for this user");
            }

            var existfavorites = await _db.Favorites
                                     .Where(f => f.UserId == userId)
                                     .Include(p => p.place)
                                     .ThenInclude(p => p.Listimage.OrderBy(i => i.ImageId))
                                     .Where(p => p.place.PlaceStatus == MyStatus.enabled)
                                     .OrderBy(f => f.FavoriteId)
                                     .ToListAsync();
            var favorites = new List<dtoFavorite>();
            foreach (var fav in existfavorites)
            {
                favorites.Add(new dtoFavorite {
                    PlaceId = fav.PlaceId,
                    PlaceName = fav.place.PlaceName,
                    BaseImageUrl = fav.place.Listimage.Count != 0 ? fav.place.Listimage.ElementAt(0).ImageUrl : null
                });
            }

            return Ok(favorites);
        }
        [HttpPost("AddFavorites{userId}")]
        public async Task<IActionResult> AddToFavorites(int userId, int placeId)
        {
            if (userId == 0 || placeId == 0)
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
            var fav = await _db.Favorites
                             .Where(u => u.UserId == userId)
                             .Include(p => p.place)
                             .Where(p => p.place.PlaceStatus == MyStatus.enabled)
                             .Where(p => p.PlaceId == placeId)
                             .FirstOrDefaultAsync();
            if (fav != null)
            {
                return BadRequest("already added");
            }
            var favorite = new Favorite { UserId = userId, PlaceId = placeId };

            existuser.Myfavorites.Add(favorite);
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
                                         .Where(f => f.PlaceId == placeId)
                                         .FirstOrDefaultAsync();

            if (existfavorite == null)
            {
                return NotFound("Favorite not found ");
            }
            _db.Favorites.Remove(existfavorite);
            await _db.SaveChangesAsync();

            return Ok("Removed from favorites.");
        }
        private  async Task<List<dtoCardPlace>> CardPlaces(List<Place> places)
        {
            var cardplaces = new List<dtoCardPlace>();
            foreach (var place in places)
            {
                cardplaces.Add(new dtoCardPlace
                {
                    PlaceId = place.PlaceId,
                    PlaceName = place.PlaceName,
                    Price = place.Price,
                    City = place.City.ToString(),
                    Visitors = place.Visitors,
                    Rating = place.Rating,
                    BaseImage = place.Listimage.Count != 0 ?place.Listimage.ElementAt(0).ImageUrl : null
                });
            }
            return cardplaces;
        }
       
    }

}
