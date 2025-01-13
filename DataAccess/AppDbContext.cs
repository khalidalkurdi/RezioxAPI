
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Model;
using Reziox.Model;
using Reziox.Model.ThePlace;
using Reziox.Model.TheUsers;

namespace Reziox.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> op) : base(op)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<EditingPlace> EditingPlaces { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PlaceImage> PlaceImages { get; set; }
        public DbSet<EditingPlaceImage> EditingPlaceImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Notification> Notifications{ get; set; }
        public DbSet<Support> Supports { get; set; }
        public DbSet<Verification> Verifications { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User()
                {
                    UserId = 1,
                    UserName = "khalid",
                    Email = "khalid@gmail.com",
                    Password = "khalid1234",
                    PhoneNumber = "0781234567",
                    City = MyCitys.zarqa,
                    DiviceToken="",
                }
            );
        }
    }
}
