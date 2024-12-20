
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
    public class FavoriteController : ControllerBase
    {
        private readonly AppDbContext _db;
       
        public FavoriteController(AppDbContext db)
        {   
            _db = db;
           
        }

        [HttpGet("Gets/{userId}")]
        public async Task<IActionResult> Gets([FromRoute] int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest("0 id is not correct !");
                }

                var existuser = await _db.Users.FirstOrDefaultAsync(u=>u.UserId==userId);
                if (existuser == null)
                {
                    return NotFound("no favorites found for this user");
                }

                var existfavorites = await _db.Favorites.Where(f => f.UserId == userId)
                                                         .Include(f => f.place)
                                                         .ThenInclude(p => p.Listimage)
                                                         .Where(p => p.place.PlaceStatus == MyStatus.approve)
                                                         .OrderBy(f => f.FavoriteId)
                                                         .ToListAsync();
                if (existfavorites.Count == 0)
                {
                    return NotFound("is not found");
                }
                var placs = new List<Place>();
                foreach (var fav in existfavorites)
                {
                    placs.Add(fav.place);
                }
                var Cardplacs = await CreateCardPlaces(placs);
                return Ok(Cardplacs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            
        }
        [HttpPost("Add/{userId}")]
        public async Task<IActionResult> Add(int userId, int placeId)
        {
            try
            {
                if (userId == 0 || placeId == 0)
                {
                    return BadRequest("0 id is not correct");
                }
                var existuser = await _db.Users.FirstOrDefaultAsync(u=>u.UserId== userId);
                if (existuser == null)
                {
                    return NotFound($" user {userId} is not exist");
                }
                var existplace = await _db.Places.FirstOrDefaultAsync(p => p.PlaceId == placeId);

                if (existplace == null)
                {
                    return NotFound($"place {placeId} is not exist");
                }
                var fav = await _db.Favorites
                                   .Where(u => u.UserId == userId)
                                   .Include(p => p.place)
                                   .Where(p => p.place.PlaceStatus == MyStatus.approve)
                                   .Where(p => p.PlaceId == placeId)
                                   .FirstOrDefaultAsync();
                if (fav != null)
                {
                    return BadRequest("already added");
                }
                var favorite = new Favorite { UserId = userId, PlaceId = placeId };

                await _db.Favorites.AddAsync(favorite);
                await _db.SaveChangesAsync();
                return Ok("added to favorites successfuly");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            
        }
        [HttpDelete("Remove/{userId}")]
        public async Task<IActionResult> Remove(int userId, int placeId)
        {
            try
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
                    return NotFound("favorite not found ");
                }
                _db.Favorites.Remove(existfavorite);
                await _db.SaveChangesAsync();

                return Ok("removed from favorites.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            
        }
        private async Task<List<dtoCardPlace>> CreateCardPlaces(IEnumerable<Place> places)
        {

            var cardplaces = new List<dtoCardPlace>();
            foreach (var place in places.OrderBy(p => Guid.NewGuid()))
            {
                cardplaces.Add(new dtoCardPlace()
                {
                    PlaceId = place.PlaceId,
                    PlaceName = place.PlaceName,
                    Price = place.Price,
                    City = place.City.ToString(),
                    Visitors = place.Visitors,
                    Rating = place.Rating,
                    BaseImage = place.Listimage.Count != 0 ? place.Listimage.OrderBy(i => i.ImageId)
                                                                            //.Where(i => i.ImageStatus == MyStatus.approve)                                                                            
                                                                            .FirstOrDefault().ImageUrl : null
                });
            }
            return cardplaces;
        }


    }

}
