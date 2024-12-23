
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
                
        [HttpPost("SmartSearch")]
        public async Task<IActionResult> SmartSearch([FromBody]dtoSearch dtoSearch)
        {
            try
            {
                var query = _db.Places
                               .Include(p => p.Listimage)
                               .Include(p => p.ListReviews)
                               .Include(p => p.Listbookings)
                               .Where(p => p.PlaceStatus == MyStatus.approve)
                               .AsQueryable();

                if (query.Count() == 0) 
                {
                    return NotFound("was not found result");
                }            
                if (dtoSearch.MinPrice!=0)
                    query = query.Where(p => p.Price >= dtoSearch.MinPrice);

                if (dtoSearch.MaxPrice != 0)
                    query = query.Where(p => p.Price <= dtoSearch.MaxPrice);

                if (dtoSearch.Gusts != 0)
                    query = query.Where(p => p.Visitors <= dtoSearch.Gusts);

                if (!string.IsNullOrEmpty(dtoSearch.City) && Enum.TryParse(dtoSearch.City, out MyCitys cityEnum))
                {
                    query = query.Where(p => p.City == cityEnum);
                }
                if (!dtoSearch.Features.IsNullOrEmpty())
                {
                    foreach (var feature in dtoSearch.Features)
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
                if (results.Count == 0)
                {
                    return NotFound("is not found");
                }
                //fillter rating
                if (dtoSearch.Rating != 0)
                {
                    foreach (var place in results)
                    {
                        if ( place.Rating < dtoSearch.Rating)
                        {
                            results.Remove(place);
                        }
                    }
                }
                //end fillter rating
                // is workeing ?
                if (!string.IsNullOrEmpty (dtoSearch.ChoicDate.ToString()) && !string.IsNullOrEmpty(dtoSearch.TypeShift))
                {
                    var daybooking = dtoSearch.ChoicDate.DayOfWeek.ToString();
                    if (!Enum.TryParse(daybooking.ToLower(), out MYDays daydate))
                    {
                        return BadRequest($"invalid day :{daybooking}");
                    }
                    query = query.Where(p => (p.WorkDays & daydate) == daydate);
                    //end is working?
                    //booked?
                    if (!Enum.TryParse(dtoSearch.TypeShift.ToLower(), out MyShifts TypeShift))
                    {
                        return BadRequest($"invalid type shift :{dtoSearch.TypeShift}");
                    }
                    foreach (var place in results)
                    {
                        var notavilable = place.Listbookings.Where(b => b.BookingDate.DayOfYear == dtoSearch.ChoicDate.DayOfYear)
                                                            .Where(b => b.StatusBooking == MyStatus.confirmation)                                                        .Where(b => (b.Typeshifts & TypeShift) == TypeShift)
                                                            .Where(b => (b.Typeshifts & TypeShift) == TypeShift)
                                                            .FirstOrDefault();                        
                        if (notavilable != null)
                        {
                            results.Remove(place);
                        }
                    }//end booked?
                }

                var cardplaces = await CreateCardPlaces(results);
                return Ok(cardplaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("Search/{Name}")]
        public async Task<IActionResult> Search([FromRoute]string? Name)
        {
            try
            {
                var existplace =await _db.Places
                                         .Include(p => p.Listimage)
                                         .Where(p => p.PlaceStatus == MyStatus.approve)
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
                    BaseImage = existplace.Listimage.Count != 0 ? existplace.Listimage.Where(i => i.ImageStatus == MyStatus.approve)
                                                                            .OrderBy(i => i.ImageId)
                                                                            .FirstOrDefault().ImageUrl:null
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
                                          .Where(p => p.PlaceStatus == MyStatus.approve)
                                          .ToListAsync();
                if (existplaces.Count == 0)
                {
                    return NotFound("not found");
                }
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
            foreach (var place in places.OrderBy(p=>Guid.NewGuid()))
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
