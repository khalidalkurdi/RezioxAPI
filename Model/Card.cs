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
        public static List<dtoCardPlace> CardPlaces(IEnumerable<Place> places)
        {

            var cardplaces = new List<dtoCardPlace>();
            foreach (var place in places.OrderBy(p => Guid.NewGuid()))
            {
                string? baseImage = place.Listimage.Count != 0 ? place.Listimage.Where(i => i.ImageStatus == MyStatus.approve)
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
        public static List<dtoCardBookingSchedule> CardBookings(List<Booking> bookings)
        {
            string rangetime = "hh:mm";
            var cardbookings = new List<dtoCardBookingSchedule>();
            foreach (var booking in bookings)
            {
                TimeSpan dif = booking.BookingDate - DateTime.UtcNow.AddHours(3);

                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.NightShift - 1}PM";
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift}PM - {booking.place.MorrningShift - 1}AM";
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.MorrningShift - 1}AM";
                }
                var days = dif.Days != 0 ? $"{dif.Days} Day & " : null;
                var hours = dif.Hours != 0 ? $"{dif.Hours}H:" : null;
                cardbookings.Add(new dtoCardBookingSchedule
                {
                    BookingId = booking.BookingId,
                    BaseImage = booking.place.Listimage.Count != 0 ? booking.place.Listimage.Where(i => i.ImageStatus == MyStatus.approve).OrderBy(i => i.ImageId).FirstOrDefault().ImageUrl : null,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToShortDateString(),
                    Time = rangetime,
                    CountDown = $"{days}{hours}{Math.Abs(dif.Minutes)}M"
                });
            }
            return cardbookings;
        }
        public static List<dtoHistory> CardUserHistory(List<Booking> bookings)
        {
            string rangetime = "hh:mm";
            var cardbookings = new List<dtoHistory>();
            foreach (var booking in bookings)
            {
                TimeSpan dif = booking.BookingDate - DateTime.UtcNow.AddHours(3);

                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.NightShift - 1}PM";
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift}PM - {booking.place.MorrningShift - 1}AM";
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.MorrningShift - 1}AM";
                }
                cardbookings.Add(new dtoHistory
                {
                    PlaceId = booking.PlaceId,
                    BookingId = booking.BookingId,
                    BaseImage = booking.place.Listimage.Count != 0 ? booking.place.Listimage.Where(i => i.ImageStatus == MyStatus.approve).OrderBy(i => i.ImageId).FirstOrDefault().ImageUrl : null,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToShortDateString(),
                    Time = rangetime,
                    CountDown = $"{dif.Days} Day"
                });
            }
            return cardbookings;
        }
        public static List<dtoCardRequsetOwner> CardOwnerRequst(List<Booking> bookings)
        {
            string rangetime = "";
            var cardbookings = new List<dtoCardRequsetOwner>();
            foreach (var booking in bookings)
            {

                if (booking.Typeshifts == MyShifts.morning)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.NightShift - 1}PM";
                }
                if (booking.Typeshifts == MyShifts.night)
                {
                    rangetime = $"{booking.place.NightShift}PM - {booking.place.MorrningShift - 1}AM";
                }
                if (booking.Typeshifts == MyShifts.full)
                {
                    rangetime = $"{booking.place.MorrningShift}AM - {booking.place.MorrningShift - 1}AM";
                }
                cardbookings.Add(new dtoCardRequsetOwner
                {
                    BookingId = booking.BookingId,
                    PlaceId = booking.PlaceId,
                    UserId = booking.UserId,
                    UserName = booking.user.UserName,
                    BaseImage = booking.user.UserImage,
                    PlaceName = booking.place.PlaceName,
                    BookingDate = booking.BookingDate.ToShortDateString(),
                    Time = rangetime,
                    IsApproved = booking.StatusBooking == MyStatus.approve ? true : false
                });
            }
            return cardbookings;
        }
    }
}
