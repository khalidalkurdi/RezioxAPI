
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;

namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaceController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PlaceController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet("SearchPlaces")]
        public async Task<IActionResult> SearchPlaces(string? keyword, string? city, string? type, int? minPrice, int? maxPrice)
        {
            var query = _db.Places.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p => p.PlaceName.Contains(keyword));

            if (!string.IsNullOrEmpty(city) && Enum.TryParse(city, out Citys cityEnum))
            {
                query = query.Where(p => p.City == cityEnum);
            }
            if (!string.IsNullOrEmpty(type) && Enum.TryParse(type, out Types typeEnum))
            {
                query = query.Where(p => p.Type == typeEnum);
            }
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);

            var results = await query.ToListAsync();
            return Ok(results);
        }
        [HttpGet("GetSuggestPlace")]
        public async Task<IActionResult> GetSuggestPlace(string city)
        {
            if (string.IsNullOrEmpty(city))
            {
                return BadRequest("city is not defind");
            }
            if(Enum.TryParse(city, out Citys cityEnum))
            {var suggestlist = await _db.Places
                .Where(p => p.City == cityEnum)
                .ToListAsync();
                return Ok(suggestlist);
            }
            return BadRequest("city not valid");
            
            
        }
        [HttpGet("GetPlaceDetails")]
        public async Task<IActionResult> GetPlaceDetails(int id)
        {
            var place = await _db.Places
                .Include(p => p.listimage)
                .Include(p => p.reviews)
                .Include(p => p.availabledays)
                .FirstOrDefaultAsync(p => p.PlaceId == id);

            if (place == null)
            {
                return NotFound();
            }

            return Ok(place);
        }
        [HttpPost("AddPlace")]
        public async Task<IActionResult> AddPlace([FromBody]Place place, IFormFile? form)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.Places.Add(place);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("EditPlace")]
        public async Task<IActionResult> EditPlace([FromBody] Place updatedPlace)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var existingPlace = await _db.Places.FirstOrDefaultAsync(p=>p.PlaceId==updatedPlace.PlaceId);
            if (existingPlace == null)
            {
                return NotFound();
            }
            //update fields
            existingPlace.PlaceName = updatedPlace.PlaceName;
            existingPlace.City = updatedPlace.City;
            existingPlace.Type = updatedPlace.Type;
            existingPlace.Status = updatedPlace.Status;
            existingPlace.Description = updatedPlace.Description;
            existingPlace.Price = updatedPlace.Price;
            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("RemovePlace")]
        public async Task<IActionResult> RemovePlace(int id)
        {
            var place = await _db.Places.FirstOrDefaultAsync(p=>p.PlaceId==id);
            if (place == null)
            {
                return NotFound();
            }

            _db.Places.Remove(place);
            await _db.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("GetPlaceById")]
        public async Task<IActionResult> GetPlaceById(int id)
        {
            var place = await _db.Places.FirstOrDefaultAsync(p=>p.PlaceId==id);
            if (place == null)
            {
                return NotFound();
            }
            return Ok(place);
        }
    }

}
