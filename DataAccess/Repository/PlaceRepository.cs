using DataAccess.Repository.ExternalcCloud;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;
using System.Linq.Expressions;
using static System.Net.Mime.MediaTypeNames;

namespace Rezioxgithub.DataAccess.Repository
{
    public class PlaceRepository : IPlaceRepository
    {
        private readonly AppDbContext _db;
        private readonly ICloudImag _cloudImag;

        public PlaceRepository(AppDbContext db , ICloudImag cloudImag)
        {
            _db = db;
            _cloudImag = cloudImag;
        }

        public  async Task<dtoDetailsPlace> Get(Expression<Func<Place, bool>> filter)
        {
            var existplace = await _db.Places.Where(filter)
                                             .Include(p => p.Listimage)
                                             .Include(p => p.ListReviews)
                                             .Where(p => p.PlaceStatus == MyStatus.enabled)
                                             .FirstOrDefaultAsync();
            if (existplace == null)
            {
                throw new Exception($"place was not found."); ;
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
                    dtoDetailsPlace.ListImage.Add(image.ImageUrl);
                }
            }
            return dtoDetailsPlace;
        }
        public Task<IEnumerable<dtoCardPlace>> GetRange(Expression<Func<Place, bool>> filter)
        {
            throw new NotImplementedException();
        }
        public async Task Add(dtoAddPlace placePost, ICollection<IFormFile> placImages)
        {
            if (!Enum.TryParse(placePost.City.ToLower(), out MyCitys cityEnum))
            {
                throw new Exception($"invalid city :{placePost.City}");
            }
            var place = new Place
            {
                PlaceName = placePost.PlaceName,
                PlacePhone = placePost.PlacePhone,
                OwnerId = placePost.OwnerId,
                City = cityEnum,
                LocationUrl = placePost.LocationUrl,
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
                   throw new Exception($"invalid day: {day}");
                }
            }
            //uploaded images

            foreach (var image in placImages)
            {
                var imageUrl = await _cloudImag.SaveImageAsync(image);
                if (imageUrl == null)
                {
                   throw new Exception($"invalid upload image {image}");
                }
                var placeImage = new PlaceImage
                {
                    PlaceId = place.PlaceId,
                    ImageUrl = imageUrl
                };
                place.Listimage.Add(placeImage);
            }
        }
        public async Task Disable(Expression<Func<Place, bool>> filter)
        {
            var existplace = await _db.Places
                                    .Where(filter)
                                    .Where(p => p.PlaceStatus == MyStatus.pending || p.PlaceStatus == MyStatus.enabled)
                                    .FirstOrDefaultAsync();                        
            if (existplace == null)
            {
                throw new Exception("is not found or already disabled");
            }
            existplace.PlaceStatus = MyStatus.disabled;
        }
        public async Task Enable(Expression<Func<Place, bool>> filter)
        {
            var existplace = await _db.Places
                                    .Where(filter)
                                    .Where(p => p.PlaceStatus == MyStatus.pending || p.PlaceStatus == MyStatus.disabled)
                                    .FirstOrDefaultAsync();
            if (existplace == null)
            {
                throw new Exception("is not found or already enabled");
            }
            existplace.PlaceStatus = MyStatus.enabled;
        }
        public async Task Update(dtoUpdatePlace updateplace , ICollection<IFormFile> images)
        {
            var existplace = await _db.Places
                                       .Include(p => p.Listimage.OrderBy(i => i.ImageId))
                                       .Where(p => p.PlaceStatus == MyStatus.enabled)
                                       .FirstOrDefaultAsync(p => p.PlaceId == updateplace.PlaceId);
            if (existplace == null || updateplace.PlaceId == 0)
            {
                throw new Exception("this place is not found");
            }
            if (!Enum.TryParse(updateplace.City.ToLower(), out MyCitys cityEnum))
            {
                throw new Exception($"invalid city :{updateplace.City}");
            }
            //update felds
            existplace.PlaceName = updateplace.PlaceName;
            existplace.PlacePhone = updateplace.PlacePhone;
            existplace.City = cityEnum;
            existplace.LocationUrl = updateplace.LocationUrl;
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
                    throw new Exception($"invalid day: {day}");
                }
            }
            //uploaded images         **  check for just add new image !!!

            foreach (var image in images)
            {
                var imageUrl = await _cloudImag.SaveImageAsync(image);
                if (imageUrl == null)
                {
                    throw new Exception($"invalid upload image {image}");
                }
                var placeImage = new PlaceImage
                {
                    PlaceId = existplace.PlaceId,
                    ImageUrl = imageUrl
                };
                existplace.Listimage.Add(placeImage);
            }
        }
        public async Task<List<dtoCardPlace>> CreateCardPlaces(List<Place> places)
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
                    BaseImage = place.Listimage.Count != 0 ? place.Listimage.FirstOrDefault().ImageUrl : null
                });
            }
            return cardplaces;           
        }

    }
}
