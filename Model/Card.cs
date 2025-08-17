using Reziox.Model.ThePlace;
using Reziox.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.DTO;

namespace Model
{
    public class Card
    {
        /// <summary>
        /// take list of Place
        /// </summary>
        /// <param name="places"></param>
        /// <returns>  list of Plac card for screen  </returns>
        public static List<dtoCardPlace> CardPlaces(List<Place> places)
        {

            var cardplaces = new List<dtoCardPlace>();
            foreach (var place in places.OrderBy(p => Guid.NewGuid()))
            {
                string? baseImage = place.Listimage.Count != 0 ? place.Listimage
                                                                           .OrderBy(i => i.ImageId)
                                                                           .FirstOrDefault()?.ImageUrl : null;
                if (baseImage == null)
                {
                    baseImage = @"https://res.cloudinary.com/dlgg1ugp1/image/upload/v1735173216/exterior-residential-house-with-summer-green-yard-shurb-trees-blue-sky_288411-1778_steusl.avif";
                }
                cardplaces.Add(new dtoCardPlace
                {
                    PlaceId = place.PlaceId,
                    PlaceName = place.PlaceName,
                    Price = place.Price,
                    City = place.City.ToString(),
                    Visitors = place.Visitors,
                    Rating = place.Rating,
                    BaseImage = baseImage
                });
            }
            return cardplaces;
        }
        /// <summary>
        /// take list of place 
        /// </summary>
        /// <param name="places"></param>
        /// <returns>lsit of card of pending place </returns>
        public static List<dtoCardPlace> CardEditingPlaces(List<EditingPlace> places)
        {

            var cardplaces = new List<dtoCardPlace>();
            foreach (var place in places)
            {
                string? baseImage = place.Listimage.Count != 0 ? place.Listimage
                                                                       .OrderBy(i => i.ImageId)
                                                                       .FirstOrDefault()?.ImageUrl : null;
                if (baseImage == null)
                {
                    baseImage = @"https://res.cloudinary.com/dlgg1ugp1/image/upload/v1735173216/exterior-residential-house-with-summer-green-yard-shurb-trees-blue-sky_288411-1778_steusl.avif";
                }
                cardplaces.Add(new dtoCardPlace
                {
                    PlaceId = place.EditingPlaceId,
                    PlaceName = place.PlaceName,
                    Price = place.Price,
                    City = place.City.ToString(),
                    Visitors = place.Visitors,
                    BaseImage = baseImage
                });
            }
            return cardplaces;
        }
        /// <summary>
        /// take list of booking
        /// </summary>
        /// <param name="bookings"></param>
        /// <returns> cards of bookings for screen </returns>
        public static List<dtoCardBookingSchedule> CardBookings(List<Booking> bookings)
        {
            string rangetime = "hh:mm";
            var cardbookings = new List<dtoCardBookingSchedule>();
            foreach (var booking in bookings)
            {
                TimeSpan dif = booking.BookingDate - DateTime.UtcNow.AddHours(3);

                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift} AM - {booking.place.NightShift - 13} PM"; //if -12 convert 24 to 12 
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift-12} PM - {booking.place.MorrningShift - 1} AM";
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{booking.place.MorrningShift} AM - {booking.place.MorrningShift - 1} AM";
                }
                var days = dif.Days > 0 ? $"{dif.Days} Day & " : null;
                var hours = dif.Hours > 0 ? $"{dif.Hours}h & " : null;
                cardbookings.Add(new dtoCardBookingSchedule
                {
                    BookingId = booking.BookingId,
                    BaseImage = booking.place.Listimage.Count != 0 ? booking.place.Listimage.OrderBy(i => i.ImageId).FirstOrDefault().ImageUrl : null,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToString("yyyy-MM-dd"),
                    Time = rangetime,
                    CountDown = $"{days}{hours}{Math.Abs(dif.Minutes)}m"
                });
            }
            return cardbookings;
        }
        /// <summary>
        /// take list of bookings
        /// </summary>
        /// <param name="bookings"></param>
        /// <returns>card of history screen</returns>
        public static List<dtoHistory> CardUserHistory(List<Booking> bookings)
        {
            string rangetime = "hh:mm";
            var cardbookings = new List<dtoHistory>();
            foreach (var booking in bookings)
            {
                TimeSpan dif = booking.BookingDate - DateTime.UtcNow.AddHours(3);

                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift} AM - {booking.place.NightShift - 13} PM";
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift - 12} PM - {booking.place.MorrningShift - 1} AM";
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{booking.place.MorrningShift} AM - {booking.place.MorrningShift - 1} AM";
                }
                cardbookings.Add(new dtoHistory
                {
                    PlaceId = booking.PlaceId,
                    BookingId = booking.BookingId,
                    BaseImage = booking.place.Listimage.Count != 0 ? booking.place.Listimage.OrderBy(i => i.ImageId).FirstOrDefault().ImageUrl : null,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToString("yyyy-MM-dd"),
                    Time = rangetime,
                    CountDown = $"{dif.Days} Day"
                });
            }
            return cardbookings;
        }
        /// <summary>
        /// take list of bookings
        /// </summary>
        /// <param name="bookings"></param>
        /// <returns>list of booking for owner panel of booking</returns>
        public static List<dtoCardRequsetOwner> CardOwnerRequst(List<Booking> bookings)
        {
            string rangetime = "";
            var cardbookings = new List<dtoCardRequsetOwner>();
            foreach (var booking in bookings)
            {

                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift} AM - {booking.place.NightShift - 13} PM";
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift-12} PM - {booking.place.MorrningShift - 1}AM";
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{booking.place.MorrningShift} AM - {booking.place.MorrningShift - 1} AM";
                }
                cardbookings.Add(new dtoCardRequsetOwner
                {
                    BookingId = booking.BookingId,
                    PlaceId = booking.PlaceId,
                    UserId = booking.UserId,
                    UserName = booking.user.UserName,
                    BaseImage = booking.user.UserImage,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToString("yyyy-MM-dd"),
                    Time = rangetime,
                    IsApproved = booking.StatusBooking == MyStatus.approve ? true : false
                });
            }
            return cardbookings;
        }
        /// <summary>
        /// take list of bookings
        /// </summary>
        /// <param name="bookings"></param>
        /// <returns> list of card bookings for user in screen requset</returns>
        public static List<dtoCardRequsetUser> CardUserRequst(List<Booking> bookings)
        {
            var cardBookings = new List<dtoCardRequsetUser>();
            foreach (var booking in bookings)
            {
                cardBookings.Add(new dtoCardRequsetUser
                {
                    PlaceId = booking.PlaceId,
                    BaseImage = booking.place.Listimage.Count != 0 ? booking.place.Listimage.OrderBy(i => i.ImageId).FirstOrDefault().ImageUrl : null,
                    PlaceName = booking.place.PlaceName,
                    Status = booking.StatusBooking.ToString(),
                    City = booking.place.City.ToString()
                });
            }
            return cardBookings;
        }
    }
}
