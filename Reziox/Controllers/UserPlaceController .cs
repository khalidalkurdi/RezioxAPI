﻿
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Model.DTO;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;
using Rezioxgithub.Model.DTO;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        [HttpGet("GetPlace{placeid}")]
        public async Task<IActionResult> GetPlaceById(int placeid)
        {
            if (placeid == 0)
            {
                return BadRequest(" 0 id is not correct !");
            }
            var existplace = await _db.Places.Where(p => p.PlaceId == placeid)
                                             .Include(p=>p.Listimage.OrderBy(i=>i.ImageId))
                                             .Include(p=>p.ListReviews)
                                             .Include(p=>p.user)
                                             .Where(p=>p.PlaceStatus==MyStatus.enabled)
                                             .FirstOrDefaultAsync();
            if (existplace == null)
            {
                return NotFound($"place {placeid} not found."); ;
            }
            
            var dtodetailsplace = new dtoDetailsPlace {
                PlaceId = existplace.PlaceId,
                PlaceName = existplace.PlaceName,
                Price = existplace.Price,
                City = existplace.City.ToString(),
                LocationUrl=existplace.LocationUrl,
                Visitors = existplace.Visitors,
                PlacePhone = existplace.user.PhoneNumber,
                Rating = existplace.Rating,
                CountReviews = existplace.CountReviews,
                Description = existplace.Description,
                MasterRoom = existplace.MasterRoom,
                BedRoom = existplace.BedRoom,
                AllBeds = existplace.Beds,
                BathRoom = existplace.BathRoom,
                Shower = existplace.Shower               
            };
            if (existplace.Listimage.Count != 0 )
            {
                foreach (var image in existplace.Listimage)
                {
                    dtodetailsplace.ListImage.Add(image.ImageUrl);
                }
            }
            dtodetailsplace.Features = ConvertFeaturesToString(existplace).Result;
            return Ok(dtodetailsplace);
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
                                       .Where(p => p.PlaceStatus == MyStatus.enabled)
                                       .Include(p=>p.Listimage.OrderBy(i=>i.ImageId))
                                       .ToListAsync();
            var cardplaces= await CreateCardPlaces(suggestlist);
            return Ok(cardplaces);
        }
        [HttpGet("MostPlaces")]
        public async Task<IActionResult> GetMostPlace()
        {
            
            var mostplaces = await _db.Places
                                       .Where(p => p.PlaceStatus == MyStatus.enabled)
                                       .Include(p=>p.ListReviews)
                                       .Include(p => p.Listimage.OrderBy(i => i.ImageId))
                                       .OrderByDescending(p=>p.Rating)
                                       .ToListAsync();
            if(mostplaces.Count == 0)
            {
                return NotFound("is not found now !");
            }
            foreach (var place in mostplaces) 
            {
                if (place.Rating<4 /*&& place.CountReviews>10*/)
                {
                    mostplaces.Remove(place);
                }
            }
            if (mostplaces.Count == 0)
            {
                return NotFound("is not found now !");
            }
            var cardplaces = await CreateCardPlaces(mostplaces);
            return Ok(cardplaces);
        }
        [HttpGet("Search")]
        public async Task<IActionResult> SearchPlaces(DateOnly choicdate, int? minPrice, int? maxPrice, int? gusts, string typeshift, string? city,ICollection<string>? features)
        {
            var query = _db.Places
                           .Include(p => p.Listimage.OrderBy(i => i.ImageId))
                           .Where(p => p.PlaceStatus == MyStatus.enabled)
                           .AsQueryable(); 
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
                return BadRequest($"invalid day :{ daybooking}");
            }
            query = query.Where(p => (p.WorkDays & daydate) != daydate);
            //end is working?
            //booked?
            if (!Enum.TryParse(typeshift.ToLower(), out MyShifts Typeshift))
            {
                return BadRequest($"invalid type shift :{typeshift}");
            }
            foreach (var place in results)
            {
                var notavilable = await _db.Bookings.Where(b => b.PlaceId == place.PlaceId)
                                        .Where(b => b.BookingDate.DayOfYear == choicdate.DayOfYear)
                                        .Where(b => (b.Typeshifts & Typeshift) == Typeshift)
                                        .Where(b=>b.StatusBooking==MyStatus.enabled)
                                        .FirstOrDefaultAsync();
                if (notavilable!=null)
                {
                    results.Remove(place);
                }
            }//end booked?

            var cardplaces = CreateCardPlaces(results).Result;
            return Ok(cardplaces);
        }
        private  async Task<List<dtoCardPlace>> CreateCardPlaces(List<Place> places)
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
        private  async Task<List<string>> ConvertFeaturesToString(Place place)
        {
            var features = new List<string>();
            if(place.WiFi) features.Add("Wi-Fi");
            if (place.PaymentByCard ) features.Add("Payment By Card");
            if(place.AirConditioning ) features.Add("Air Conditioning");
            if(place.Barbecue ) features.Add("Barbecue");
            if(place.EventArea) features.Add("Event Area");
            if(place.ChildrensPlayground) features.Add("Childrens Playground");
            if(place.ChildrensPool) features.Add("Childrens Pool");
            if(place.Parking) features.Add("Parking");
            if(place.Jacuzzi) features.Add("Jacuzzi");
            if(place.HeatedSwimmingPool) features.Add("HeatedSwimming Pool");
            if(place.Football) features.Add("Football");
            if(place.BabyFoot) features.Add("BabyFoot");
            if(place.Ballpool) features.Add("Ballpool");
            if(place.Tennis) features.Add("Tennis");
            if(place.Volleyball) features.Add("Volleyball");
       
            return features;
        }
    }

}