﻿
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;


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

        [HttpGet("Get/{userId}")]
        public async Task<IActionResult> Get([FromRoute] int userId)
        {
            try
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
                                         .ThenInclude(p => p.Listimage)
                                         .Where(p => p.place.PlaceStatus == MyStatus.enabled)
                                         .OrderBy(f => f.FavoriteId)
                                         .ToListAsync();
                var favorites = new List<dtoCardPlace>();
                foreach (var fav in existfavorites)
                {
                    favorites.Add(new dtoCardPlace
                    {
                        PlaceId = fav.PlaceId,
                        PlaceName = fav.place.PlaceName,
                        BaseImage = fav.place.Listimage.Count != 0 ? fav.place.Listimage.OrderBy(i => i.ImageId).FirstOrDefault().ImageUrl : null
                    });
                }

                return Ok(favorites);
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
                var existuser = await _db.Users.FindAsync(userId);
                if (existuser == null)
                {
                    return NotFound($" user {userId} is not exist");
                }
                var existplace = await _db.Places.FindAsync(placeId);
                if (existplace == null)
                {
                    return NotFound($"place {placeId} is not exist");
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
        
       
    }

}