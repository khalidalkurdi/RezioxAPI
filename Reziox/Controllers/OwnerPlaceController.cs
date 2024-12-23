using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model.ThePlace;
using Reziox.Model;
using Microsoft.EntityFrameworkCore;


namespace RezioxAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerPlaceController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly Cloudinary _cloudinary;
        public OwnerPlaceController(AppDbContext db, Cloudinary cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
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
                    var imageUrl = await SaveImageAsync(image);
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
                await _db.EditingPlaces.AddAsync(thisplace);
                await SentNotificationAsync(dtoPlace.OwnerId, "Waiting Confirmation", $"Your chalete is Pending ,admin will check  it soon.. !");
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
                //check if have not any booking        &&      if have it can not deleted
                var existbookins = await _db.Bookings
                                        .Where(p => p.PlaceId == existplace.PlaceId)
                                        .Where(p => p.BookingDate.DayOfYear >= DateTime.UtcNow.DayOfYear)
                                        .AnyAsync();
                if (existbookins)
                {
                    await SentNotificationAsync(existplace.OwnerId, "Confirmation of impossibility", $"Can not delet your chalete because it has bookings!");
                    return BadRequest("it has bookings!");
                }
                //end check if have not any booking

                existplace.PlaceStatus = MyStatus.reject;
                await SentNotificationAsync(existplace.OwnerId, "Confirm Delet", $"your chalete{existplace.PlaceName} is deleted !");
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

                var ownerplaces = await _db.Places
                                           .Where(p => p.OwnerId == ownerId)
                                           .Where(p => p.PlaceStatus == MyStatus.approve)
                                           .Include(p => p.Listimage.OrderBy(i => i.ImageId))
                                           .OrderBy(p => p.PlaceId)
                                           .ToListAsync();
                if (ownerplaces.Count == 0)
                {
                    return NotFound("not found");
                }
                var cardplaces = await CreateCardPlaces(ownerplaces);
                return Ok(cardplaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task SentNotificationAsync(int userid,string title, string message)
        {
            await _db.Notifications.AddAsync(new Notification { UserId = userid,Title=title, Message = message });
        }
        private async Task<string> SaveImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return null;
            //requst
            using var stream = image.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(image.FileName, stream)
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
                return null;
            return uploadResult.SecureUrl.ToString();
        }
        private async Task<List<dtoCardPlace>> CreateCardPlaces(List<Place> places)
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
                    BaseImage = place.Listimage.Count != 0 ?place.Listimage.FirstOrDefault().ImageUrl : null
                });
            }
            return cardplaces;
        }
        
    }
}
