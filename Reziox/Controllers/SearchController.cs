
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;

namespace Reziox.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SearchPlaceController : ControllerBase
    {
        private readonly AppDbContext _db;
       
        public SearchPlaceController(AppDbContext db)
        {   
            _db = db;
           
        }
                
        [HttpGet("SmartSearch")]
        public async Task<IActionResult> SmartSearch(DateOnly choicdate, int? minPrice, int? maxPrice, int? gusts, string typeshift, string? city,ICollection<string>? features)
        {
            try
            {
                var query = _db.Places
               .Include(p => p.Listimage)
               .Where(p => p.PlaceStatus == MyStatus.enabled)
               .AsQueryable();
                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice);
                if (maxPrice.HasValue)
                    query = query.Where(p => p.Price <= maxPrice);
                if (gusts.HasValue)
                    query = query.Where(p => p.Visitors <= gusts);
                if (!string.IsNullOrEmpty(city) && Enum.TryParse(city, out MyCitys cityEnum))
                {
                    query = query.Where(p => p.City == cityEnum);
                }
                if (!features.IsNullOrEmpty())
                {
                    foreach (var feature in features)
                    {
                        switch (feature.ToLower())
                        {
                            case "wifi": query = query.Where(p => p.WiFi == true); break;
                            case "paymentbycard": query = query.Where(p => p.PaymentByCard == true); break;
                            case "airconditioning": query = query.Where(p => p.AirConditioning == true); break;
                            case "barbecue": query = query.Where(p => p.Barbecue == true); break;
                            case "eventarea": query = query.Where(p => p.EventArea == true); break;
                            case "childrensplayground": query = query.Where(p => p.ChildrensPlayground == true); break;
                            case "childrenspool": query = query.Where(p => p.ChildrensPool == true); break;
                            case "parking": query = query.Where(p => p.Parking == true); break;
                            case "jacuzzi": query = query.Where(p => p.Jacuzzi == true); break;
                            case "heatedswimmingpool": query = query.Where(p => p.HeatedSwimmingPool == true); break;
                            case "football": query = query.Where(p => p.Football == true); break;
                            case "babyfoot": query = query.Where(p => p.BabyFoot == true); break;
                            case "ballpool": query = query.Where(p => p.Ballpool == true); break;
                            case "tennis": query = query.Where(p => p.Tennis == true); break;
                            case "volleyball": query = query.Where(p => p.Volleyball == true); break;
                        }
                    }
                }
                var results = await query.ToListAsync();

                // is workeing ?
                var daybooking = choicdate.DayOfWeek.ToString();
                if (!Enum.TryParse(daybooking.ToLower(), out MYDays daydate))
                {
                    return BadRequest($"invalid day :{daybooking}");
                }
                query = query.Where(p => (p.WorkDays & daydate) != daydate);
                //end is working?
                //booked?
                if (!Enum.TryParse(typeshift.ToLower(), out MyShifts Typeshift))
                {
                    return BadRequest($"invalid type shift :{typeshift}");
                }
                foreach (var place in results)
                {
                    var notavilable = await _db.Bookings.Where(b => b.PlaceId == place.PlaceId)
                                            .Where(b => b.BookingDate.DayOfYear == choicdate.DayOfYear)
                                            .Where(b => (b.Typeshifts & Typeshift) == Typeshift)
                                            .Where(b => b.StatusBooking == MyStatus.enabled)
                                            .FirstOrDefaultAsync();
                    if (notavilable != null)
                    {
                        results.Remove(place);
                    }
                }//end booked?

                var cardplaces = CreateCardPlaces(results).Result;
                return Ok(cardplaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("Search")]
        public async Task<IActionResult> Search(string? Name)
        {
            try
            {
                var existplace =await _db.Places
                                         .Include(p => p.Listimage)
                                         .Where(p => p.PlaceStatus == MyStatus.enabled)
                                         .Where(p => p.PlaceName == Name)
                                         .FirstOrDefaultAsync();
                var cardplace = new dtoCardPlace
                {
                    PlaceId = existplace.PlaceId,
                    PlaceName = existplace.PlaceName,
                    Price = existplace.Price,
                    City = existplace.City.ToString(),
                    Visitors = existplace.Visitors,
                    Rating = existplace.Rating,
                    BaseImage = existplace.Listimage.Count != 0 ? existplace.Listimage.FirstOrDefault().ImageUrl : null
                };
                return Ok(cardplace);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("SuggestSearch")]
        public async Task<IActionResult> Suggests()
        {
            try
            {
                var existplaces = await _db.Places                                         
                                          .Where(p => p.PlaceStatus == MyStatus.enabled)
                                          .ToListAsync();
                var suggests = new List<string>();
                foreach (var place in existplaces)
                {
                    suggests.Add(place.PlaceName);
                }
                
                return Ok(suggests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        private  async Task<List<dtoCardPlace>> CreateCardPlaces(List<Place> places)
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
                    BaseImage = place.Listimage.Count != 0 ?place.Listimage.OrderBy(i => i.ImageId).FirstOrDefault().ImageUrl : null
                });
            }
            return cardplaces;
        }      
    }

}
