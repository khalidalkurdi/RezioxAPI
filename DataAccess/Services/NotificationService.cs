using DataAccess.ExternalcCloud;
using FirebaseAdmin.Messaging;
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
        public async Task SentAsync(string diviceToken, int userid, string title, string alert)
        {
            try
            {

                var message = new Message()
                {
                    Token = diviceToken,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body =alert
                    }
                };
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
                await _db.Notifications.AddAsync(new Reziox.Model.Notification { UserId = userid, Title = title, Message = alert });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send notification: {ex.Message}");            
            }            
        }
    }
}
