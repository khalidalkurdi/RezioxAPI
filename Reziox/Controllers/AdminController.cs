using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;
using static System.Net.Mime.MediaTypeNames;
using DataAccess.PublicClasses;
using Model;
using DataAccess.ExternalcCloud;


namespace RezioxAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly INotificationService _notification;
        private readonly ICloudImag _cloudImag;


        public AdminController(AppDbContext db,INotificationService notification,ICloudImag cloudImag)
        {
            _db = db;
            _notification = notification;
            _cloudImag = cloudImag;
        }
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            var existUsers = await _db.Users                                                                            
                                      .OrderBy(u => u.UserId)
                                      .ToListAsync();
            if (existUsers.Count == 0)
            {
                return NotFound("is not found");
            }
            return Ok(existUsers);
        }
        [HttpGet("GetNotifiactions")]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {              
                var existnotifications = await _db.Notifications    
                    //order form new to old
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
                if (existnotifications.Count == 0)
                {
                    return NotFound("no notifications found for this user.");
                }
                var dtoNotifications = new List<dtoNotification>();
                foreach (var notification in existnotifications)
                {
                    var difDate = DateTime.UtcNow.AddHours(3) - notification.CreatedAt ;
                    var countdown = difDate.Days > 0 ? $"{difDate.Days} day " : $"{difDate.Hours} hour";
                    dtoNotifications.Add(new dtoNotification {NotificationId=notification.NotificationId, Title = notification.Title, Message = notification.Message, CreatedAt = $"From {countdown} in {notification.CreatedAt.ToString("yyyy:M:dd & h:mm tt")}" });
                }

                return Ok(dtoNotifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetEnableplaces")]
        public async Task<IActionResult> GetEnablePlaces()
        {
            var existPlaces = await _db.Places
                                    .Where(p => p.PlaceStatus == MyStatus.approve)
                                    .Include(p => p.Listimage)
                                    .Include(p => p.ListReviews)                                    
                                    .Include(u => u.user)
                                    .OrderBy(p=>p.PlaceId)
                                    .ToListAsync();
            if (existPlaces.Count == 0)
            {
                return NotFound("is not found");
            }
            var cardsPlaces = Card.CardPlaces(existPlaces);
            return Ok(cardsPlaces);
        }
        [HttpGet("GetDisabledplaces")]
        public async Task<IActionResult> GetDisabledplaces()
        {
            var existPlaces = await _db.Places
                                    .Where(p => p.PlaceStatus == MyStatus.reject)
                                    .Include(p => p.Listimage)
                                    .Include(p => p.ListReviews)                                    
                                    .Include(u => u.user)
                                    .OrderBy(p => p.PlaceId)
                                    .ToListAsync();
            if (existPlaces.Count() == 0)
            {
                return NotFound("is not found");
            }
            var cardsPlaces = Card.CardPlaces(existPlaces);
            return Ok(cardsPlaces);
        }
        [HttpGet("GetPendingplaces")]
        public async Task<IActionResult> GetPendingPlaces()
        {
            var existPlaces = await _db.EditingPlaces                                   
                                        .Where(p => p.PlaceStatus == MyStatus.pending)
                                        .Include(p =>p.Listimage)                                                                                                                       
                                        .Include(p=>p.user)
                                        .OrderBy(p=>p.PlaceId)
                                        .ToListAsync();
            if (existPlaces.Count == 0)
            {
                return NotFound("is not found");
            }
            var cardsPlaces = Card.CardEditingPlaces(existPlaces);
            return Ok(cardsPlaces);
        }
        [HttpDelete("DeletePlace/{placeId}")]
        public async Task<IActionResult> DeletePlace(int placeId)
        {
            if (placeId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existplace = await _db.Places
                                    .Where(p => p.PlaceId == placeId)
                                    .FirstOrDefaultAsync();
            if (existplace == null)
            {
                return NotFound("is not found");
            }
            _db.Places.Remove(existplace);
            var existOwner = await _db.Users.AsNoTracking().Where(u => u.UserId == existplace.OwnerId).FirstOrDefaultAsync();
            if(existOwner != null)
            {
                 await _notification.SentAsync(existOwner.DiviceToken,existOwner.UserId, "Delete Confirmation", "The admin delete your chalet..",MyScreen.None);
            }
            await _db.SaveChangesAsync();
            return Ok("place deleted succfuly!");
        }
        [HttpGet("EnablePlace/{editingPlaceId}")]
        public async Task<IActionResult> EnablePlace(int editingPlaceId)
        {
            if (editingPlaceId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existEditingPlace = await _db.EditingPlaces
                                            .Where(p => p.EditingPlaceId == editingPlaceId)
                                            .Where(p =>  p.PlaceStatus == MyStatus.pending)
                                            .Include(p=>p.Listimage)
                                            .FirstOrDefaultAsync();
            if (existEditingPlace == null)
            {
                return NotFound("is not found or approve");
            }            
            if (existEditingPlace.PlaceId != 0 && existEditingPlace.PlaceId !=null)
            {                
                var existplace = await _db.Places
                                           .Where(p => p.PlaceId == existEditingPlace.PlaceId)
                                           .Where(p=>p.PlaceStatus==MyStatus.approve)
                                           .Include(p=>p.Listimage)
                                           .FirstOrDefaultAsync();
                if (existplace == null)
                {
                    return NotFound("this place is not found");
                }
                //update felds
                existplace.PlaceName = existEditingPlace.PlaceName;
                existplace.PlacePhone = existEditingPlace.PlacePhone;
                existplace.City = existEditingPlace.City;
                existplace.Visitors = existEditingPlace.Visitors;
                existplace.LocationUrl = existEditingPlace.LocationUrl;
                existplace.Description = existEditingPlace.Description;
                existplace.Price = existEditingPlace.Price;
                existplace.Firstpayment = existEditingPlace.Firstpayment;
                existplace.WorkDays = existEditingPlace.WorkDays;
                existplace.NightShift = existEditingPlace.NightShift;
                existplace.MorrningShift = existEditingPlace.MorrningShift;               
                existplace.MasterRoom = existEditingPlace.MasterRoom;
                existplace.BedRoom = existEditingPlace.BedRoom;
                existplace.Beds = existEditingPlace.Beds;
                existplace.BathRoom = existEditingPlace.BathRoom;
                existplace.Shower = existEditingPlace.Shower;
                existplace.WiFi = existEditingPlace.WiFi;
                existplace.PaymentByCard= existEditingPlace.PaymentByCard;
                existplace.AirConditioning = existEditingPlace.AirConditioning;
                existplace.Barbecue = existEditingPlace.Barbecue;
                existplace.EventArea = existEditingPlace.EventArea;
                existplace.ChildrensPlayground = existEditingPlace.ChildrensPlayground;
                existplace.ChildrensPool = existEditingPlace.ChildrensPool;
                existplace.Parking = existEditingPlace.Parking;
                existplace.Jacuzzi = existEditingPlace.Jacuzzi;
                existplace.HeatedSwimmingPool = existEditingPlace.HeatedSwimmingPool;
                existplace.Football = existEditingPlace.Football;
                existplace.BabyFoot = existEditingPlace.BabyFoot;
                existplace.Ballpool = existEditingPlace.Ballpool;
                existplace.Tennis = existEditingPlace.Tennis;
                existplace.Volleyball = existEditingPlace.Volleyball;
                //end update felds                
                //delete old image !
                foreach (var image in existplace.Listimage)
                {
                    _db.PlaceImages.Remove(image);               
                }
                //end delete old image !
                //uploaded images       
                foreach (var image in existEditingPlace.Listimage.OrderBy(i=>i.ImageId))
                {
                    existplace.Listimage.Add(new PlaceImage
                    {                        
                        PlaceId=existplace.PlaceId,
                        ImageUrl=image.ImageUrl,                        
                    });
                }
            }
            else
            {
                var newPlace = new Place
                {
                    PlaceName = existEditingPlace.PlaceName,
                    PlacePhone = existEditingPlace.PlacePhone,
                    OwnerId = existEditingPlace.OwnerId,
                    City = existEditingPlace.City,
                    LocationUrl = existEditingPlace.LocationUrl,
                    Description = existEditingPlace.Description,
                    Price = existEditingPlace.Price,
                    Firstpayment=existEditingPlace.Firstpayment,
                    Visitors = existEditingPlace.Visitors,
                    WorkDays=existEditingPlace.WorkDays,
                    NightShift = existEditingPlace.NightShift,
                    MorrningShift = existEditingPlace.MorrningShift,
                    PaymentByCard = existEditingPlace.PaymentByCard,
                    MasterRoom = existEditingPlace.MasterRoom,
                    BedRoom = existEditingPlace.BedRoom,
                    Beds = existEditingPlace.Beds,
                    BathRoom = existEditingPlace.Beds,
                    Shower = existEditingPlace.Shower,
                    WiFi = existEditingPlace.WiFi,
                    AirConditioning = existEditingPlace.AirConditioning,
                    Barbecue = existEditingPlace.Barbecue,
                    EventArea = existEditingPlace.EventArea,
                    ChildrensPlayground = existEditingPlace.ChildrensPlayground,
                    ChildrensPool = existEditingPlace.ChildrensPool,
                    Parking = existEditingPlace.Parking,
                    Jacuzzi = existEditingPlace.Jacuzzi,
                    HeatedSwimmingPool = existEditingPlace.HeatedSwimmingPool,
                    Football = existEditingPlace.Football,
                    BabyFoot = existEditingPlace.BabyFoot,
                    Ballpool = existEditingPlace.Ballpool,
                    Tennis = existEditingPlace.Tennis,
                    Volleyball = existEditingPlace.Volleyball
                };
                foreach (var image in existEditingPlace.Listimage)
                {
                    newPlace.Listimage.Add(new PlaceImage
                    {                        
                        PlaceId = newPlace.PlaceId,
                        ImageUrl = image.ImageUrl,
                    });
                }
               await _db.Places.AddAsync(newPlace);
            }
            existEditingPlace.PlaceStatus = MyStatus.approve;
            var existOwner = await _db.Users.AsNoTracking().Where(u => u.UserId == existEditingPlace.OwnerId).FirstOrDefaultAsync();
            if(existOwner != null)
            {
                 await _notification.SentAsync(existOwner.DiviceToken, existOwner.UserId, "Acceptance Confirmation", "The admin accept your chalet and it added to your chalets",MyScreen.OwnerPlaces);
            }
            await _db.SaveChangesAsync();
            return Ok("place approve succfuly!");
        }
        [HttpGet("DisablePlace/{editingPlaceId}")]
        public async Task<IActionResult> DisablePlace(int editingPlaceId)
        {
            if (editingPlaceId == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var existplace = await _db.EditingPlaces
                                    .Where(p => p.PlaceId == editingPlaceId)
                                    .Where(p => p.PlaceStatus == MyStatus.pending)
                                    .FirstOrDefaultAsync();
            if (existplace == null)
            {
                return NotFound("is not found or already disabled");
            }
            existplace.PlaceStatus = MyStatus.reject;
            var existOwner = await _db.Users.AsNoTracking().Where(u => u.UserId == existplace.OwnerId).FirstOrDefaultAsync();
            if (existOwner != null)
            {
                 await _notification.SentAsync(existOwner.DiviceToken, existOwner.UserId, "Rejection Confirmation", "The admin reject your chalet", MyScreen.None);
            }            
            await _db.SaveChangesAsync();
            return Ok("place disabled succfuly!");
        }
        [HttpGet("GetEnableBookings")]
        public async Task<IActionResult> GetEnableBookings()
        {
            var existbookings = await _db.Bookings
                                        .Where(b => b.StatusBooking == MyStatus.confirmation)
                                        .Include(u => u.user)
                                        .Include(b=>b.place)
                                        .OrderBy(b => b.BookingId)
                                        .ToListAsync();
            if (existbookings.Count == 0)
            {
                return NotFound("is not found");
            }
            return Ok(existbookings);
        }
        [HttpGet("GetRequsetUsersBookings")]
        public async Task<IActionResult> GetRequsetUsersBookings()
        {
            var existbookings = await _db.Bookings
                                    .Where(b => b.StatusBooking == MyStatus.reject)
                                    .Include(u => u.user)
                                    .Include(b => b.place)
                                    .OrderBy(b => b.BookingId)
                                    .ToListAsync();
            if (existbookings.Count() == 0)
            {
                return NotFound("is not found");
            }
            var cardBookings = new List<dtoCardRequsetUser>();
            foreach (var booking in existbookings)
            {
                string? baseImage = booking.place.Listimage.Count != 0 ? booking.place.Listimage
                                                                           .OrderBy(i => i.ImageId)
                                                                           .FirstOrDefault()?.ImageUrl : null;
                if (baseImage == null)
                {
                    baseImage = @"https://res.cloudinary.com/dlgg1ugp1/image/upload/v1735173216/exterior-residential-house-with-summer-green-yard-shurb-trees-blue-sky_288411-1778_steusl.avif";
                }
                cardBookings.Add(new dtoCardRequsetUser
                {
                    PlaceId = booking.PlaceId,
                    BaseImage = baseImage,
                    PlaceName = booking.place.PlaceName,
                    Status = booking.StatusBooking.ToString(),
                    City = booking.place.City.ToString()
                });
            }
            return Ok(cardBookings);
        }
        [HttpGet("GetRequsetOwnerBookings")]
        public async Task<IActionResult> GetRequsetOwnerBookings()
        {
            var existbookings = await _db.Bookings
                                        .Where(b => b.StatusBooking == MyStatus.pending || b.StatusBooking == MyStatus.approve)
                                        .Include(u => u.user)
                                        .Include(b => b.place)
                                        .OrderBy(b => b.BookingId)
                                        .ToListAsync();
            if (existbookings.Count == 0)
            {
                return NotFound("is not found");
            }
            var cardBookings = Card.CardOwnerRequst(existbookings);
            return Ok(cardBookings);
        }
        

    }

}
