using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model.ThePlace;
using Reziox.Model;
using Microsoft.EntityFrameworkCore;
using Model.ThePlace;

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
        [HttpPost("Adddir")]
        public async Task<IActionResult> Adddir([FromForm] dtoAddPlace placePost , ICollection<IFormFile> images)
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
                    PlacePhone=placePost.PlacePhone,
                    OwnerId = placePost.OwnerId,
                    City = cityEnum,
                    LocationUrl=placePost.LocationUrl,
                    Description = placePost.Description,
                    Price = placePost.Price,
                    Visitors= placePost.Visitors,
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
                await SentNotificationAsync(place.OwnerId, "Waiting Confirmation", $"Your chalete is Pending ,admin will check  it soon.. !");
                await _db.SaveChangesAsync();
                return Ok("place sent to admin");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("Editdir")]
        public async Task<IActionResult> Editdir([FromForm] dtoUpdatePlace updateplace , ICollection<IFormFile> images)
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
                                         .Where(p => p.PlaceStatus == MyStatus.approve)
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
                existplace.PlacePhone = updateplace.PlacePhone;
                existplace.City = cityEnum;
                existplace.Visitors = updateplace.Visitors;
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
                //delete old image !
                foreach (var image in existplace.Listimage)
                {
                    image.PlaceId = 0;
                }
                //end delete old image !
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
                await SentNotificationAsync(existplace.OwnerId, "Waiting Confirmation", $"Your update is Pending ,admin will check it soon.. !");
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
        [HttpGet("GetPlaces")]
        public async Task<IActionResult> OwnerPlaces(int ownerId)
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
