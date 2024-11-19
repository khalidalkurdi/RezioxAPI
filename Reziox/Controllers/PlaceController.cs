
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;
using Rezioxgithub.Model.DTO;
using Rezioxgithub.Model.ThePlace;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Reziox.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PlaceController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly Cloudinary _cloudinary ;
        public PlaceController(AppDbContext db , Cloudinary cloudinary)
        {   
            _db = db;
            _cloudinary = cloudinary;
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
        [HttpPost("Add")]
        public async Task<IActionResult> AddPlace([FromForm] PlaceDto placePost, ICollection<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (images == null || images.Count<2)
            {
                return BadRequest("please , upload at least  5 images for your place ");
            }
            if (!Enum.TryParse(placePost.City.ToLower(),out MyCitys cityEnum)) 
            {
                return BadRequest("invalid City");
            } 
            var place = new Place
            {
                PlaceName=placePost.PlaceName,
                OwnerId=placePost.OwnerId,
                City =cityEnum,
                Description=placePost.Description, 
                Price=placePost.Price,
                NightShift=placePost.NightShift,
                MorrningShift=placePost.MorrningShift,
                PaymentByCard=placePost.PaymentByCard,
                MasterRoom=placePost.MasterRoom,
                BedRoom=placePost.BedRoom,
                Beds=placePost.Beds,
                BathRoom=placePost.Beds,
                Shower=placePost.Shower,
                WiFi=placePost.WiFi,
                AirConditioning=placePost.AirConditioning,
                Barbecue=placePost.Barbecue,
                EventArea=placePost.EventArea,
                ChildrensPlayground=placePost.ChildrensPlayground,
                ChildrensPool=placePost.ChildrensPool,
                Parking=placePost.Parking,
                Jacuzzi=placePost.Jacuzzi,
                HeatedSwimmingPool=placePost.HeatedSwimmingPool,
                Football=placePost.Football,
                BabyFoot=placePost.BabyFoot,
                Ballpool=placePost.Ballpool,
                Tennis=placePost.Tennis,
                Volleyball=placePost.Volleyball
            };

            //parse work day to falgs
            foreach(var day in placePost.WorkDays)
            {
                if (Enum.TryParse(day,out MYDays parsedDay))
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
                    return BadRequest(image);
                }
                var placeImage = new PlaceImage
                {
                    PlaceId = place.PlaceId,
                    ImageUrl = imageUrl
                };
                await _db.PlaceImages.AddAsync(placeImage);
            }
            await _db.Places.AddAsync(place);
            var notificationOwner = new Notification
            {
                UserId = place.OwnerId,
                Message = $"your chalete is Pending ,it will check soon.. !",
                CreatedAt = DateTime.Now
            };
            await _db.Notifications.AddAsync(notificationOwner);
            await _db.SaveChangesAsync();
            return Ok("place sent to admin");
        }
        [HttpPut("Edit")]//try create class for edit
        public async Task<IActionResult> EditPlace([FromBody] Place updatedPlace)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var existingPlace = await _db.Places.FirstOrDefaultAsync(p=>p.PlaceId==updatedPlace.PlaceId);
            if (existingPlace == null)
            {
                return NotFound($"place {updatedPlace.PlaceId} not found."); ;
            }
            //update fields
            existingPlace.PlaceName = updatedPlace.PlaceName;
            existingPlace.City = updatedPlace.City;
            existingPlace.Status = updatedPlace.Status;
            existingPlace.Description = updatedPlace.Description;
            existingPlace.Price = updatedPlace.Price;
            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("Remove/{placeid}")]
        public async Task<IActionResult> RemovePlace(int placeid)
        {
            if(placeid == 0)
            {
                return BadRequest(" 0 id is not correct !");
            }
            var place = await _db.Places.FirstOrDefaultAsync(p=>p.PlaceId==placeid);
            if (place == null)
            {
                return NotFound($"place {placeid} not found."); ;
            }
            //check if have not any booking        &&      if have it can not deleted
            

            //end check if have not any booking
            _db.Places.Remove(place);
            var notificationOwner = new Notification
            {
                UserId = place.OwnerId,
                Message = $"your chalete is deleted  !",
                CreatedAt = DateTime.Now
            };
            await _db.Notifications.AddAsync(notificationOwner);
            await _db.SaveChangesAsync();
            return Ok("place deleted successfuly ! ");
        }
        [HttpGet("Place{placeid}")]
        public async Task<IActionResult> GetPlaceById(int placeid)
        {
            if (placeid == 0)
            {
                return BadRequest(" 0 id is not correct !");
            }
            var place = await _db.Places
                                 .Include(p=>p.Listimage.OrderBy(i=>i.ImageId))   
                                 .FirstOrDefaultAsync(p=>p.PlaceId==placeid);
            if (place == null)
            {
                return NotFound($"place {place.PlaceId} not found."); ;
            }        
            return Ok(place);
        }
        [HttpGet("Search")]
        public async Task<IActionResult> SearchPlaces(DateTime choicdate, int? minPrice, int? maxPrice, int? gusts, string typeshift, string? city,ICollection<string>? features)
        {
            var query = _db.Places.AsQueryable(); 
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
                        case "wifi" : query = query.Where(p => p.WiFi == true); break;
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
                return BadRequest(daybooking);
            }
            query = query.Where(p => (p.WorkDays & daydate) != daydate);
            //end is working?
            //booked?
            if (!Enum.TryParse(typeshift.ToLower(), out MyShifts Typeshift))
            {
                return BadRequest(typeshift);
            }
            foreach (var place in results)
            {
                var notavilable = await _db.Bookings.Where(b => b.PlaceId == place.PlaceId)
                                        .Where(b => b.BookingDate.DayOfYear == choicdate.DayOfYear)
                                        .Where(b => (b.Typeshifts & Typeshift) == Typeshift)
                                        .FirstOrDefaultAsync();
                if (notavilable!=null)
                {
                    results.Remove(place);
                }
            }//end booked?
            return Ok(results);
        }
        [HttpGet("Places{city}")]
        public async Task<IActionResult> GetSuggestPlace(string city)
        {
            if (string.IsNullOrEmpty(city))
            {
                return BadRequest("city is null or empty");
            }
            if(!Enum.TryParse(city.ToLower(), out MyCitys cityEnum))
            {
                return BadRequest("city not valid");
            }
            var suggestlist = await _db.Places
                                       .Where(p => p.City == cityEnum)
                                       .Include(p=>p.Listimage.OrderBy(i=>i.ImageId))
                                       .ToListAsync();
            return Ok(suggestlist);
        }
        [HttpGet("Details/{placeid}")]
        public async Task<IActionResult> GetPlaceDetails(int placeid)
        {
            if(placeid == 0)
            {
                return BadRequest("0 id is not correct !");
            }
            var place = await _db.Places
                                 .Include(p => p.Listimage.OrderBy(i => i.ImageId))
                                 .FirstOrDefaultAsync(p => p.PlaceId == placeid);

            if (place == null)
            {
                return NotFound($"place {placeid} is not founf");
            }
            return Ok(place);
        }

    }

}
