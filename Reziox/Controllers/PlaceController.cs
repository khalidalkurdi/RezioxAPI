
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;
using Rezioxgithub.Model.ThePlace;

namespace Reziox.Controllers
{
    //khalid
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
        public async Task<IActionResult> AddPlace([FromBody] PlaceVM placePost,ICollection<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (images == null || images.Count<2)
            {
                return BadRequest("please , Upload at least  2 Images for your place ");
            }
            if (!Enum.TryParse(placePost.City.ToLower(),out Citys cityEnum)) 
            {
                return BadRequest("invalid City");
            } 
            if (Enum.TryParse(placePost.Type.ToLower(), out Types typesEnum)) 
            {
                return BadRequest("invalid Type");
            }
            
            var place = new Place
            {
                PlaceName=placePost.PlaceName,
                OwnerId=placePost.OwnerId,
                City =cityEnum,
                Type = typesEnum,
                Description=placePost.Description, 
                Price=placePost.Price
            };
            foreach(var day in placePost.availabledays)
            {
                Enum.TryParse(day, out DaysofWeek dayEnum);
                var availableDay = new AvailableDay
                {
                    PlaceId = place.PlaceId,
                    Day = dayEnum
                };
                place.availabledays.Add(availableDay);
            }
            //add part time to table
            foreach (var part in placePost.partsTime)
            {              
                var partTime = new PartTime
                {
                    PlaceId=place.PlaceId,
                    Start=part.Start,
                    End=part.End,
                };
                place.partsTime.Add(partTime);
            }
            //uploaded images
            foreach (var image in images)
            {              
                var imageUrl = await SaveImageAsync(image);
                var placeImage = new PlaceImage
                {
                    PlaceId = place.PlaceId,
                    ImageUrl = imageUrl
                };
                place.listimage.Add(placeImage);
            }
            _db.Places.Add(place);
            await _db.SaveChangesAsync();

            return Ok();
        }

        private async Task<string> SaveImageAsync(IFormFile image)
        {// here api upload
            return null;
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
