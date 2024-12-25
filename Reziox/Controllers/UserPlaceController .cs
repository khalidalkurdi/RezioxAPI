
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
    public class UserPlaceController : ControllerBase
    {
        private readonly AppDbContext _db;
       
        public UserPlaceController(AppDbContext db)
        {   
            _db = db;
           
        }
        [HttpGet("Details/{placeId}/{userId}")]
        public async Task<IActionResult> Details([FromRoute] int placeId, int userId)
        {
            try
            {
                if (placeId == 0 || userId==0)
                {
                    return BadRequest(" 0 id is not correct !");
                }
                var existplace = await _db.Places.AsNoTracking()
                                                 .Where(p => p.PlaceId == placeId)
                                                 .Include(p => p.Listimage)                                                
                                                 .Include(p => p.ListReviews)
                                                 .Where(p => p.PlaceStatus == MyStatus.approve)
                                                 .FirstOrDefaultAsync();
                if (existplace == null)
                {
                    return NotFound($"place {placeId} not found.");
                }

                var dtoDetailsPlace = new dtoDetailsPlace
                {
                    PlaceId = existplace.PlaceId,
                    PlaceName = existplace.PlaceName,
                    PlacePhone = existplace.PlacePhone,                   
                    City = existplace.City.ToString(),
                    LocationUrl = existplace.LocationUrl,
                    Description = existplace.Description,
                    MorrningShift = existplace.MorrningShift,
                    NightShift= existplace.NightShift,
                    Visitors=existplace.Visitors,
                    Price = existplace.Price,
                    Firstpayment = existplace.Firstpayment,
                    PaymentByCard = existplace.PaymentByCard,
                    MasterRoom = existplace.MasterRoom,
                    BedRoom = existplace.BedRoom,
                    Beds = existplace.Beds,
                    BathRoom = existplace.Beds,
                    Shower = existplace.Shower,
                    WiFi = existplace.WiFi,
                    AirConditioning = existplace.AirConditioning,
                    Barbecue = existplace.Barbecue,
                    EventArea = existplace.EventArea,
                    ChildrensPlayground = existplace.ChildrensPlayground,
                    ChildrensPool = existplace.ChildrensPool,
                    Parking = existplace.Parking,
                    Jacuzzi = existplace.Jacuzzi,
                    HeatedSwimmingPool = existplace.HeatedSwimmingPool,
                    Football = existplace.Football,
                    BabyFoot = existplace.BabyFoot,
                    Ballpool = existplace.Ballpool,
                    Tennis = existplace.Tennis,
                    Volleyball = existplace.Volleyball                   
                };
                // check is favorited
                var existfavorite = await _db.Favorites.Where(f => f.PlaceId == placeId)                                                 
                                                       .Where(f => f.UserId==userId)
                                                       .FirstOrDefaultAsync();
                if (existfavorite != null)
                {
                    dtoDetailsPlace.Favorited = true;
                }
                //end check is favorited
                //convert days from flag to string
                if ((existplace.WorkDays & MYDays.sunday) == MYDays.sunday)
                    dtoDetailsPlace.WorkDays.Add(MYDays.sunday.ToString());

                if ((existplace.WorkDays & MYDays.monday) == MYDays.monday)
                    dtoDetailsPlace.WorkDays.Add(MYDays.monday.ToString());

                if ((existplace.WorkDays & MYDays.tuesday) == MYDays.tuesday)
                    dtoDetailsPlace.WorkDays.Add(MYDays.tuesday.ToString());

                if ((existplace.WorkDays & MYDays.wednesday) == MYDays.wednesday)
                    dtoDetailsPlace.WorkDays.Add(MYDays.wednesday.ToString());

                if ((existplace.WorkDays & MYDays.thursday) == MYDays.thursday)
                    dtoDetailsPlace.WorkDays.Add(MYDays.thursday.ToString());

                if ((existplace.WorkDays & MYDays.friday) == MYDays.friday)
                    dtoDetailsPlace.WorkDays.Add(MYDays.friday.ToString());

                if ((existplace.WorkDays & MYDays.saturday) == MYDays.saturday)
                    dtoDetailsPlace.WorkDays.Add(MYDays.saturday.ToString());
                //end convert
                if (existplace.Listimage.Count != 0)
                {                    
                    foreach (var image in existplace.Listimage.OrderBy(i => i.ImageId).Where(i => i.ImageStatus == MyStatus.approve))
                    {                        
                        dtoDetailsPlace.ListImage.Add(image.ImageUrl);                        
                    }
                }                
                return Ok(dtoDetailsPlace);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            
        }
        [HttpGet("Suggests/{userId}")]
        public async Task<IActionResult> GetSameZone([FromRoute] int userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest(" 0 id is not correct !");
                }
                var existuser = await _db.Users
                                        .Where(u => u.UserId == userId)
                                        .FirstOrDefaultAsync();
                if(existuser == null)
                {
                    return NotFound(" user not found");
                }
                var suggestlist = await _db.Places.AsNoTracking()
                                           .Where(p => p.City==existuser.City)
                                           .Where(p => p.PlaceStatus == MyStatus.approve)
                                           .Include(p => p.Listimage)
                                           .ToListAsync();
                if (suggestlist.Count == 0)
                {
                    return Ok(suggestlist);
                }
                var cardplaces = await CreateCardPlaces(suggestlist);
                return Ok(cardplaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("Mosts")]
        public async Task<IActionResult> GetMost()
        {
            try
            {
                var mostplaces = await _db.Places.AsNoTracking()
                                       .Where(p => p.PlaceStatus == MyStatus.approve)
                                       .Include(p => p.ListReviews)
                                       .Include(p => p.Listimage)
                                       .ToListAsync();
                if (mostplaces.Count == 0)
                {
                    return Ok(mostplaces);
                }
                /*      edit after add reviwes
                foreach (var place in mostplaces)
                {
                    if (place.Rating < 4 && place.CountReviews>10)
                    {
                        mostplaces.Remove(place);
                    }
                }
                if (mostplaces.Count == 0)
                {
                    return NotFound("is not found now !");
                }*/
                var cardplaces = await CreateCardPlaces(mostplaces);
                return Ok(cardplaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            
        }
        [HttpGet("Randomly")]
        public async Task<IActionResult> GetRandomly()
        {
            try
            {
                var existplaces = await _db.Places.AsNoTracking()
                                       .Where(p => p.PlaceStatus == MyStatus.approve)
                                       .Include(p => p.Listimage)
                                       .ToListAsync();
                if (existplaces.Count == 0)
                {
                    return NotFound("is not found !");
                }
                var randomplaces = existplaces.Take(30)
                                              .ToList();
                var cardPlaces = await CreateCardPlaces(randomplaces);
                return Ok(cardPlaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
        private  async Task<List<dtoCardPlace>> CreateCardPlaces(IEnumerable<Place> places)
        {

            var cardplaces = new List<dtoCardPlace>();
            foreach (var place in places.OrderBy(p => Guid.NewGuid()))
            {
                cardplaces.Add(new dtoCardPlace
                {
                    PlaceId = place.PlaceId,
                    PlaceName = place.PlaceName,
                    Price = place.Price,
                    City = place.City.ToString(),
                    Visitors = place.Visitors,
                    Rating = place.Rating,
                    BaseImage = place.Listimage.Count != 0 ?place.Listimage.Where(i=>i.ImageStatus==MyStatus.approve)
                                                                            .OrderBy(i=>i.ImageId)
                                                                            .FirstOrDefault().ImageUrl : null
                });
            }
            return cardplaces;
        }
       
    }

}
