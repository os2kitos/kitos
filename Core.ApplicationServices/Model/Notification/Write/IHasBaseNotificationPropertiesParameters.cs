using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public interface IHasBaseNotificationPropertiesParameters
    {
        public BaseNotificationPropertiesModificationParameters BaseProperties { get; }
    }
}
