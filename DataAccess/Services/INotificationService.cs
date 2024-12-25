using Reziox.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.PublicClasses
{
    public interface INotificationService
    {
       Task SentAsync(string DiviceToken,int userid, string title, string message);
        
    }
}
