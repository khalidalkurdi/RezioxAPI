using DataAccess.ExternalcCloud;
using Reziox.DataAccess;
using Reziox.Model;

namespace DataAccess.PublicClasses
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        public NotificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task SentAsync(int userid, string title, string message)
        {
            await _db.Notifications.AddAsync(new Notification { UserId = userid, Title = title, Message = message });           
        }
    }
}
