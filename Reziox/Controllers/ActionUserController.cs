﻿
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.ThePlace;

namespace Reziox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionUserController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ActionUserController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications(int userId)
        {
            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                //order form new to old
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            if (notifications == null)
            {
                return NotFound("No notifications found for this user.");
            }

            return Ok(notifications);
        }

        [HttpGet("GetFavorites")]
        public async Task<IActionResult> GetFavorites(int userId)
        {
            var favorites = await _db.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.place)
                .ToListAsync();

            if (favorites==null)
            {
                return NotFound("No favorites found for this user");
            }

            return Ok(favorites);
        }
        [HttpPost("AddToFavorites")]
        public async Task<IActionResult> AddToFavorites(int userId, int placeId)
        {
            var favorite = new Favorite { UserId = userId, PlaceId = placeId };
            _db.Favorites.Add(favorite);
            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpDelete("RemoveFromFavorites")]
        public async Task<IActionResult> RemoveFromFavorites(int userId, int placeId)
        {
            var favorite = await _db.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.PlaceId == placeId);

            if (favorite == null)
            {
                return NotFound("Favorite not found.");
            }

            _db.Favorites.Remove(favorite);
            await _db.SaveChangesAsync();

            return Ok("Removed from favorites.");
        }

        [HttpPost("AddBooking")]
        public async Task<IActionResult> AddBooking(int placeId, int userId,DateTime date)
        {   //find exist place and user
            var existplace = await _db.Places.FirstOrDefaultAsync(p=>p.PlaceId==placeId);
            var existuser = await _db.Users.FirstOrDefaultAsync(u=>u.UserId==userId);
            if(existplace == null||existuser ==null)
            {
                return NotFound("User or Place not found.");
            }
            var booking = new Booking { PlaceId = existplace.PlaceId, UserId = existuser.UserId, BookingDate=date};
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();
            var notificationOwner = new Notification
            {
                UserId = existplace.OwnerId,
                Message = $"A place you own is reserved on date{date}",
                CreatedAt = DateTime.Now
            };
            _db.Notifications.Add(notificationOwner);
            var notificationUser = new Notification
            {
                UserId = existuser.UserId,
                Message = $"Your reservation has been successfully received on date {date}",
                CreatedAt = DateTime.Now
            };
            _db.Notifications.Add(notificationUser);
            await _db.SaveChangesAsync();

            return Ok("Booking canceled successfully");
        }

        [HttpDelete("CancelBooking")]
        public async Task<IActionResult> CancelBooking(int bookingId , DateTime date)
        {
            var existbooking = await _db.Bookings.FindAsync(bookingId);
            if (existbooking == null)
            {
                return NotFound("Booking not found.");
            }
            //condtion for cancle
            if(existbooking.BookingDate.Day==date.Day)
            {
                return Ok("can not cancle");
            }
            _db.Bookings.Remove(existbooking);
            await _db.SaveChangesAsync();
            //add notifiacation for  owner and user
            var place = await _db.Places.FindAsync(existbooking.PlaceId);
            if (place == null)
            {
                return NotFound("Place not found");
            }
            var notificationOwner = new Notification
            {
                UserId = place.OwnerId,
                Message = $"A booking at your place has been canceled for the date {existbooking.BookingDate}",
                CreatedAt = DateTime.Now
            };
            _db.Notifications.Add(notificationOwner);
            var notificationUser = new Notification
            {
                UserId = existbooking.UserId,
                Message = $"You canceled the booking on date {existbooking.BookingDate}",
                CreatedAt = DateTime.Now
            };
            _db.Notifications.Add(notificationUser);
            await _db.SaveChangesAsync();
            return Ok("Booking canceled successfully.");
        }
        [HttpGet("GetBookingsForUser")]
        public async Task<IActionResult> GetBookingsForUser(int userId)
        {
            var bookings = await _db.Bookings
                .Where(b => b.UserId == userId)
                .Include(u=>u.user)
                .Include(b => b.place)
                .OrderDescending()
                .ToListAsync();
            if (bookings==null)
            {
                return NotFound();
            }
            return Ok(bookings);
        }
        [HttpGet("GetBookingsForOwner")]
        public async Task<IActionResult> GetBookingsForOwner(int OwnerId)
        {
            var bookings = await _db.Bookings
                .Where(p => p.place.OwnerId == OwnerId)
                .Include(u => u.user)
                .Include(b => b.place)
                .OrderDescending()
                .ToListAsync();
            if (bookings == null)
            {
                return NotFound();
            }
            return Ok(bookings);
        }
        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview(int userId, int placeId, int rating)
        {
            // Check if User and Place exist
            var existuser = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            var existplace = await _db.Places.FirstOrDefaultAsync(p => p.PlaceId == placeId);
            if (existuser == null || existplace == null)
            {
                return NotFound("User or Place not found.");
            }
            var review = new Review { PlaceId = placeId, UserId = userId, Rating = rating};
            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}