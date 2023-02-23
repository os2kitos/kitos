using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices.Model.Notification
{
    public interface IHasBaseNotificationPropertiesModel
    {
        public BaseNotificationPropertiesModel BaseProperties { get; set; }
    }
}
