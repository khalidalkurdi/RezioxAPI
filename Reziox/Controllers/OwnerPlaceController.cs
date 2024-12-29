using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model.ThePlace;
using Reziox.Model;
using Microsoft.EntityFrameworkCore;
using DataAccess.ExternalcCloud;
using DataAccess.PublicClasses;
using Model;


namespace RezioxAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerPlaceController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ICloudImag _cloudImag;
        private readonly INotificationService _notification;
        public OwnerPlaceController(AppDbContext db, ICloudImag cloudImag,INotificationService notification)
        {
            _db = db;
            _cloudImag = cloudImag;
            _notification = notification;
        }
        [HttpPost("UpSert")]
        public async Task<IActionResult> Upsert([FromForm] dtoEditingPlace dtoPlace, ICollection<IFormFile> images)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (images == null || images.Count < 5)
                {
                    return BadRequest("please , upload at least  5 images for your place ");
                }
                if (images.Count>20)
                {
                    return BadRequest("please , upload less than 20 images for your place");
                }
                if (!Enum.TryParse(dtoPlace.City.ToLower(), out MyCitys cityEnum))
                {
                    return BadRequest($"invalid city :{dtoPlace.City}");
                }
                var thisplace= new EditingPlace
                {
                    PlaceId = dtoPlace.PlaceId,
                    PlaceName = dtoPlace.PlaceName,
                    PlacePhone = dtoPlace.PlacePhone,
                    OwnerId = dtoPlace.OwnerId,
                    City = cityEnum,
                    LocationUrl = dtoPlace.LocationUrl,
                    Description = dtoPlace.Description,
                    Price = dtoPlace.Price,
                    Firstpayment=dtoPlace.Firstpayment,
                    Visitors = dtoPlace.Visitors,
                    NightShift = dtoPlace.NightShift,
                    MorrningShift = dtoPlace.MorrningShift,
                    MasterRoom = dtoPlace.MasterRoom,
                    BedRoom = dtoPlace.BedRoom,
                    Beds = dtoPlace.Beds,
                    BathRoom = dtoPlace.BathRoom,
                    Shower = dtoPlace.Shower,
                    WiFi = dtoPlace.WiFi,
                    PaymentByCard = dtoPlace.PaymentByCard,
                    AirConditioning = dtoPlace.AirConditioning,
                    Barbecue = dtoPlace.Barbecue,
                    EventArea = dtoPlace.EventArea,
                    ChildrensPlayground = dtoPlace.ChildrensPlayground,
                    ChildrensPool = dtoPlace.ChildrensPool,
                    Parking = dtoPlace.Parking,
                    Jacuzzi = dtoPlace.Jacuzzi,
                    HeatedSwimmingPool = dtoPlace.HeatedSwimmingPool,
                    Football = dtoPlace.Football,
                    BabyFoot = dtoPlace.BabyFoot,
                    Ballpool = dtoPlace.Ballpool,
                    Tennis = dtoPlace.Tennis,
                    Volleyball = dtoPlace.Volleyball          //30
                };

                //parse work day to falgs
                foreach (var day in dtoPlace.WorkDays)
                {
                    if (Enum.TryParse(day.ToLower(), out MYDays parsedDay))
                    {
                        thisplace.WorkDays |= parsedDay; // Combine flags
                    }
                    else
                    {
                        return BadRequest($"invalid day: {day}");
                    }
                }
                //uploaded images

                foreach (var image in images)
                {
                    var imageUrl = await _cloudImag.SaveImageAsync(image);
                    if (imageUrl == null)
                    {
                        return BadRequest($"invalid upload image {image}");
                    }
                    var placeImage = new EditingPlaceImage
                    {                        
                        EditingPlaceId=thisplace.EditingPlaceId,
                        ImageUrl = imageUrl,
                    };
                    thisplace.Listimage.Add(placeImage);
                }
                var existUser = await _db.Users.AsNoTracking().Where(u => u.UserId == dtoPlace.OwnerId).FirstOrDefaultAsync();
                await _db.EditingPlaces.AddAsync(thisplace);
                if (existUser != null)
                {
                    await _notification.SentAsync(existUser.DiviceToken,existUser.UserId, "Waiting Confirmation", $"Your chalete is Pending ,admin will check  it soon.. !", MyScreen.None);
                }
                await _db.SaveChangesAsync();
                return Ok("place sent to admin");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("Remove/{placeid}")]
        public async Task<IActionResult> Remove([FromRoute] int placeid)
        {
            try
            {
                if (placeid == 0)
                {
                    return BadRequest(" 0 id is not correct !");
                }
                var existplace = await _db.Places.Where(p => p.PlaceId == placeid).FirstOrDefaultAsync();
                if (existplace == null)
                {
                    return NotFound($"place {placeid} not found."); ;
                }
                var existUser = await _db.Users.AsNoTracking().Where(u => u.UserId == existplace.OwnerId).FirstOrDefaultAsync();
                //check if have not any booking        &&      if have it can not deleted
                var existbookins = await _db.Bookings.AsNoTracking()
                                        .Where(p => p.PlaceId == existplace.PlaceId)
                                        .Where(p => p.BookingDate >= DateTime.UtcNow.AddHours(3))
                                        .ToListAsync();
                if (existbookins.Count != 0 && existUser!=null)
                {
                    await _notification.SentAsync(existUser.DiviceToken,existUser.UserId, "Confirmation of impossibility", $"Can not delet your chalete because it has bookings!", MyScreen.OwnerSchedule);
                    return BadRequest("it has bookings!");
                }
                //end check if have not any booking 

                existplace.PlaceStatus = MyStatus.reject;
                if (existUser != null)
                {
                    await _notification.SentAsync(existUser.DiviceToken,existUser.UserId, "Confirm Delet", $"your chalete{existplace.PlaceName} is deleted !",MyScreen.None);
                }
                await _db.SaveChangesAsync();
                return Ok("place deleted successfuly ! ");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetPlaces/{ownerId}")]
        public async Task<IActionResult> GetPlaces([FromRoute] int ownerId)
        {
            try
            {
                if (ownerId == 0)
                {
                    return BadRequest("0 id is not correct");
                }

                var ownerplaces = await _db.Places.AsNoTracking()
                                                   .Where(p => p.OwnerId == ownerId)
                                                   .Where(p => p.PlaceStatus == MyStatus.approve)
                                                   .Include(p => p.Listimage.OrderBy(i => i.ImageId))
                                                   .OrderBy(p => p.PlaceId)
                                                   .ToListAsync();
                if (ownerplaces.Count == 0)
                {
                    return Ok(ownerplaces);
                }
                var cardplaces = Card.CardPlaces(ownerplaces);
                return Ok(cardplaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
