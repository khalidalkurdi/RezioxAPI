
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Model;
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

                if (!string.IsNullOrEmpty(dtoSearch.City) && Enum.TryParse(dtoSearch.City.ToLower(), out MyCitys cityEnum))
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
                if (results.Count == 0 || results==null)
                {
                    return Ok(results);
                }
                //fillter rating
                if (dtoSearch.Rating > 0)
                {
                    results.RemoveAll(p=>p.Rating < dtoSearch.Rating);                   
                }
                //end fillter rating
                if (!string.IsNullOrEmpty (dtoSearch.ChoicDate.ToString()) && !string.IsNullOrEmpty(dtoSearch.TypeShift))
                {
                    // is workeing ?
                    var daybooking = dtoSearch.ChoicDate.DayOfWeek.ToString();
                    if (!Enum.TryParse(daybooking.ToLower(), out MYDays choicDay))
                    {
                        return BadRequest($"invalid day :{daybooking}");
                    }
                    query = query.Where(p => (p.WorkDays & choicDay) == choicDay);
                    //end is working?
                    // remove places not avaliable
                    if (!Enum.TryParse(dtoSearch.TypeShift.ToLower(), out MyShifts TypeShift))
                    {
                        return BadRequest($"invalid type shift :{dtoSearch.TypeShift}");
                    }
                    foreach (var place in results.ToList())
                    {
                        var notavilable = place.Listbookings.Where(b => b.BookingDate.Date.DayOfYear == dtoSearch.ChoicDate.DayOfYear)
                                                            .Where(b => b.StatusBooking == MyStatus.confirmation)                                                        .Where(b => (b.Typeshifts & TypeShift) == TypeShift)
                                                            .Where(b => (b.Typeshifts & TypeShift) == TypeShift)
                                                            .FirstOrDefault();                        
                        if (notavilable != null)
                        {
                            results.Remove(place);
                        }
                    }//end remove 
                }

                var cardplaces = Card.CardPlaces(results);
                return Ok(cardplaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("Search/{Name}")]
        public async Task<IActionResult> Search([FromRoute]string Name)
        {
            try
            {
                var existplaces =await _db.Places
                                         .Include(p => p.Listimage)
                                         .Where(p => p.PlaceStatus == MyStatus.approve)
                                         .Where(p => p.PlaceName.Equals(Name))
                                         .ToListAsync();
                if (existplaces.Count == 0)
                {
                    return NotFound("is not found");
                }
                var cardPlaces = Card.CardPlaces(existplaces);
                return Ok(cardPlaces);
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
                    return Ok(existplaces);
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
    }

}
