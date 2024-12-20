
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
        [HttpGet("Details/{placeid}")]
        public async Task<IActionResult> Details([FromRoute] int placeid)
        {
            try
            {
                if (placeid == 0)
                {
                    return BadRequest(" 0 id is not correct !");
                }
                var existplace = await _db.Places.Where(p => p.PlaceId == placeid)
                                                 .Include(p => p.Listimage)                                                
                                                 .Include(p => p.ListReviews)
                                                 .Where(p => p.PlaceStatus == MyStatus.approve)
                                                 .FirstOrDefaultAsync();
                if (existplace == null)
                {
                    return NotFound($"place {placeid} not found."); ;
                }

                var dtoDetailsPlace = new dtoDetailsPlace
                {
                    PlaceId = existplace.PlaceId,
                    PlaceName = existplace.PlaceName,
                    PlacePhone = existplace.PlacePhone,                   
                    City = existplace.City.ToString(),
                    LocationUrl = existplace.LocationUrl,
                    Description = existplace.Description,
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
                if (existplace.Listimage.Count != 0)
                {
                    dtoDetailsPlace.ListImage = new List<string>();
                    foreach (var image in existplace.Listimage.OrderBy(i => i.ImageId))
                    {
                        if (image.ImageStatus == MyStatus.approve)
                        {
                             dtoDetailsPlace.ListImage.Add(image.ImageUrl);
                        }
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
        public async Task<IActionResult> GetSuggests([FromRoute] int userId)
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
                var suggestlist = await _db.Places
                                           .Where(p => p.City==existuser.City)
                                           .Where(p => p.PlaceStatus == MyStatus.approve)
                                           .Include(p => p.Listimage)
                                           .ToListAsync();
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
                var mostplaces = await _db.Places
                                       .Where(p => p.PlaceStatus == MyStatus.approve)
                                       .Include(p => p.ListReviews)
                                       .Include(p => p.Listimage)
                                       .ToListAsync();
                if (mostplaces.Count == 0)
                {
                    return NotFound("is not found now !");
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
                var existplaces = await _db.Places
                                       .Where(p => p.PlaceStatus == MyStatus.approve)
                                       .Include(p => p.Listimage)
                                       .ToListAsync();
                if (existplaces.Count == 0)
                {
                    return NotFound("is not found !");
                }
                var randomplaces = existplaces.OrderBy(p=>Guid.NewGuid())
                                              .Take(25)
                                              .ToList();
                var cardplaces = await CreateCardPlaces(randomplaces);
                return Ok(cardplaces);
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
