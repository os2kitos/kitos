using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Notification
{
    public class NotificationService : INotificationService
    {
        public IEnumerable<Advice> GetAdvices()
        {
            return new List<Advice>();
        }
    }
}
