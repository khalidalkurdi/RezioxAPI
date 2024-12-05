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
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromForm] dtoAddPlace placePost , ICollection<IFormFile> images)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
            
                if (images == null || images.Count<2)
                {
                    return BadRequest("please , upload at least  5 images for your place ");
                }
                if (!Enum.TryParse(placePost.City.ToLower(), out MyCitys cityEnum))
                {
                    return BadRequest($"invalid city :{placePost.City}");
                }
                var place = new Place
                {
                    PlaceName = placePost.PlaceName,
                    OwnerId = placePost.OwnerId,
                    City = cityEnum,
                    LocationUrl=placePost.LocationUrl,
                    Description = placePost.Description,
                    Price = placePost.Price,
                    NightShift = placePost.NightShift,
                    MorrningShift = placePost.MorrningShift,
                    PaymentByCard = placePost.PaymentByCard,
                    MasterRoom = placePost.MasterRoom,
                    BedRoom = placePost.BedRoom,
                    Beds = placePost.Beds,
                    BathRoom = placePost.Beds,
                    Shower = placePost.Shower,
                    WiFi = placePost.WiFi,
                    AirConditioning = placePost.AirConditioning,
                    Barbecue = placePost.Barbecue,
                    EventArea = placePost.EventArea,
                    ChildrensPlayground = placePost.ChildrensPlayground,
                    ChildrensPool = placePost.ChildrensPool,
                    Parking = placePost.Parking,
                    Jacuzzi = placePost.Jacuzzi,
                    HeatedSwimmingPool = placePost.HeatedSwimmingPool,
                    Football = placePost.Football,
                    BabyFoot = placePost.BabyFoot,
                    Ballpool = placePost.Ballpool,
                    Tennis = placePost.Tennis,
                    Volleyball = placePost.Volleyball
                };

                //parse work day to falgs
                foreach (var day in placePost.WorkDays)
                {
                    if (Enum.TryParse(day.ToLower(), out MYDays parsedDay))
                    {
                        place.WorkDays |= parsedDay; // Combine flags
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
                    var placeImage = new PlaceImage
                    {
                        PlaceId = place.PlaceId,
                        ImageUrl = imageUrl
                    };
                    place.Listimage.Add(placeImage);
                }
                await _db.Places.AddAsync(place);
                await SentNotificationAsync(place.OwnerId, $"your chalete is Pending ,admin will check  it soon.. !");
                await _db.SaveChangesAsync();
                return Ok("place sent to admin");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("Edit")]//try create class for edit
        public async Task<IActionResult> Edit([FromForm] dtoUpdatePlace updateplace , ICollection<IFormFile> images)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
            
                if (images == null || images.Count<2)
                {
                    return BadRequest("please , upload at least  5 images for your place ");
                }
                var existplace = await _db.Places
                                         .Include(p => p.Listimage.OrderBy(i => i.ImageId))
                                         .Where(p => p.PlaceStatus == MyStatus.enabled)
                                         .FirstOrDefaultAsync(p => p.PlaceId == updateplace.PlaceId);
                if (existplace == null || updateplace.PlaceId==0)
                {
                    return NotFound("this place is not found");
                }
                if (!Enum.TryParse(updateplace.City.ToLower(), out MyCitys cityEnum))
                {
                    return BadRequest($"invalid city :{updateplace.City}");
                }
                //update felds
                existplace.PlaceName = updateplace.PlaceName;              
                existplace.City = cityEnum;
                existplace.LocationUrl= updateplace.LocationUrl;
                existplace.Description = updateplace.Description;
                existplace.Price = updateplace.Price;
                existplace.NightShift = updateplace.NightShift;
                existplace.MorrningShift = updateplace.MorrningShift;
                existplace.PaymentByCard = updateplace.PaymentByCard;
                existplace.MasterRoom = updateplace.MasterRoom;
                existplace.BedRoom = updateplace.BedRoom;
                existplace.Beds = updateplace.Beds;
                existplace.BathRoom = updateplace.Beds;
                existplace.Shower = updateplace.Shower;
                existplace.WiFi = updateplace.WiFi;
                existplace.AirConditioning = updateplace.AirConditioning;
                existplace.Barbecue = updateplace.Barbecue;
                existplace.EventArea = updateplace.EventArea;
                existplace.ChildrensPlayground = updateplace.ChildrensPlayground;
                existplace.ChildrensPool = updateplace.ChildrensPool;
                existplace.Parking = updateplace.Parking;
                existplace.Jacuzzi = updateplace.Jacuzzi;
                existplace.HeatedSwimmingPool = updateplace.HeatedSwimmingPool;
                existplace.Football = updateplace.Football;
                existplace.BabyFoot = updateplace.BabyFoot;
                existplace.Ballpool = updateplace.Ballpool;
                existplace.Tennis = updateplace.Tennis;
                existplace.Volleyball = updateplace.Volleyball;
                existplace.PlaceStatus = MyStatus.pending;
                //end update felds

                //parse work day to falgs
                foreach (var day in updateplace.WorkDays)
                {
                    if (Enum.TryParse(day.ToLower(), out MYDays parsedDay))
                    {
                        existplace.WorkDays |= parsedDay; // Combine flags
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
                    var placeImage = new PlaceImage
                    {
                        PlaceId = existplace.PlaceId,
                        ImageUrl = imageUrl
                    };
                    existplace.Listimage.Add(placeImage);
                }
                await SentNotificationAsync(existplace.OwnerId, $"your update is Pending ,admin will check it soon.. !");
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
                    await SentNotificationAsync(existplace.OwnerId, $"can not delet your chalete because it has bookings!");
                    return BadRequest("it has bookings!");
                }
                //end check if have not any booking

                existplace.PlaceStatus = MyStatus.disabled;
                await SentNotificationAsync(existplace.OwnerId, $"your chalete{existplace.PlaceName} is deleted !");
                await _db.SaveChangesAsync();
                return Ok("place deleted successfuly ! ");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetPlaces")]
        public async Task<IActionResult> OwnerPlaces([FromRoute] int ownerId)
        {
            try
            {
                if (ownerId == 0)
                {
                    return BadRequest("0 id is not correct");
                }

                var ownerplaces = await _db.Places
                                           .Where(p => p.OwnerId == ownerId)
                                           .Where(p => p.PlaceStatus == MyStatus.enabled)
                                           .Include(p => p.Listimage.OrderBy(i => i.ImageId))
                                           .OrderBy(p => p.PlaceId)
                                           .ToListAsync();
                var cardplaces = CreateCardPlaces(ownerplaces).Result;
                return Ok(cardplaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task SentNotificationAsync(int userid, string message)
        {
            await _db.Notifications.AddAsync(new Notification { UserId = userid, Message = message });
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
                    BaseImage = place.Listimage.Count != 0 ?place.Listimage.ElementAt(0).ImageUrl : null
                });
            }
            return cardplaces;
        }
        private async Task<List<string>> ConvertFeaturesToString(Place place)
        {
            var features = new List<string>();
            if (place.WiFi)
                if (place.PaymentByCard) features.Add(place.PaymentByCard.ToString());
            if (place.AirConditioning) features.Add(place.AirConditioning.ToString());
            if (place.Barbecue) features.Add(place.Barbecue.ToString());
            if (place.EventArea) features.Add(place.EventArea.ToString());
            if (place.ChildrensPlayground) features.Add(place.ChildrensPlayground.ToString());
            if (place.ChildrensPool) features.Add(place.ChildrensPool.ToString());
            if (place.Parking) features.Add(place.Parking.ToString());
            if (place.Jacuzzi) features.Add(place.Jacuzzi.ToString());
            if (place.HeatedSwimmingPool) features.Add(place.HeatedSwimmingPool.ToString());
            if (place.Football) features.Add(place.Football.ToString());
            if (place.BabyFoot) features.Add(place.BabyFoot.ToString());
            if (place.Ballpool) features.Add(place.Ballpool.ToString());
            if (place.Tennis) features.Add(place.Tennis.ToString());
            if (place.Volleyball) features.Add(place.Volleyball.ToString());

            return features;
        }
    }
}
