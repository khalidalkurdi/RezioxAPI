using Reziox.Model;
using Reziox.Model.TheUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.IRepository
{
    public interface INotificationRepository
    {
        Task<Notification> GetAsync(Expression<Func<Notification, bool>> filter);
        Task<List<dtoNotification>> GetAllAsync(Expression<Func<Notification, bool>> filter);
        Task Send(int userid, string title, string message);
    }
}
