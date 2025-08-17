
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
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
        /// <summary>
        /// take userid 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>list of favorite chalets </returns>
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
                    return NotFound("no found this user");
                }

                var existfavorites = await _db.Favorites.AsNoTracking()
                                                         .Where(f => f.UserId == userId)
                                                         .Include(f => f.place)
                                                         .ThenInclude(p => p.Listimage)
                                                         .Where(p => p.place.PlaceStatus == MyStatus.approve)
                                                         .OrderBy(f => f.FavoriteId)
                                                         .ToListAsync();
                if (existfavorites.Count == 0)
                {
                    return Ok(existfavorites);
                }
                var placs = new List<Place>();
                foreach (var fav in existfavorites)
                {
                    placs.Add(fav.place);
                }
                var Cardplacs = Card.CardPlaces(placs);
                return Ok(Cardplacs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            
        }
        /// <summary>
        /// take user id and chalet id  for add the chalet to the favorite list
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="placeId"></param>
        /// <returns>status code</returns>
        [HttpPost("Add/{userId}/{placeId}")]
        public async Task<IActionResult> Add([FromRoute]int userId,int placeId)
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
        /// <summary>
        /// take user id and chalet id for remove chalet from favorite list
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="placeId"></param>
        /// <returns>status code</returns>
        [HttpDelete("Remove/{userId}/{placeId}")]
        public async Task<IActionResult> Remove([FromRoute]int userId,int placeId)
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
       


    }

}
