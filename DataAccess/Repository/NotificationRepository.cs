using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using Reziox.Model;
using Reziox.Model.TheUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _db;
        public NotificationRepository(AppDbContext db)
        {
            _db=db;
        }
        public async Task<Notification> GetAsync(Expression<Func<Notification, bool>> filter)
        {
            var existNotification = await _db.Notifications
                                            .Where(filter)              
                                            .FirstOrDefaultAsync();
            if (existNotification == null )
            {
                throw new Exception("no notifications founded for this user.");
            }
            return existNotification;
        }
        public async Task<List<dtoNotification>> GetAllAsync(Expression<Func<Notification, bool>> filter)
        {
            var existnotifications = await _db.Notifications
                                            .Where(filter)
                                            .Where(n => n.CreatedAt.DayOfYear >= DateTime.UtcNow.DayOfYear - 30)
                                            //order form new to old
                                            .OrderByDescending(n => n.CreatedAt)
                                            .ToListAsync();

            if (existnotifications.Count() == 0)
            {
                throw new Exception("no notifications founded for this user.");
            }
            var dtoNotifications = new List<dtoNotification>();
            foreach (var notification in existnotifications)
            {
                var difDate = notification.CreatedAt - DateTime.UtcNow;
                var countdown = difDate.Days < 0 ? $"{difDate.Days} day " : $"{difDate.Hours} hour";
                dtoNotifications.Add(new dtoNotification {NotificationId=notification.NotificationId, Title = notification.Title, Message = notification.Message, CreatedAt = $"{countdown}", IsRead = notification.IsRead });
            }
            return dtoNotifications;
        }
        public async Task Send(int userid, string title, string message)
        {
            await _db.Notifications.AddAsync(new Notification { UserId = userid, Title = title, Message = message });
        }
    }
}
