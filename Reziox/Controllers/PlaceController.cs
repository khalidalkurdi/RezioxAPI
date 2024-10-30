using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model;

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
        [HttpGet]
        public async Task<IActionResult> SearchPlaces(string keyword, string? city, string? type, decimal? minPrice, decimal? maxPrice)
        {
            var query = _db.Places.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(p => p.PlaceName.Contains(keyword));

            if (!string.IsNullOrEmpty(city) && Enum.TryParse<Citys>(city, out var cityEnum))
            {
                query = query.Where(p => p.City == cityEnum);
            }
            if (!string.IsNullOrEmpty(type) && Enum.TryParse<Types>(type, out var typeEnum))
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
    }
}
