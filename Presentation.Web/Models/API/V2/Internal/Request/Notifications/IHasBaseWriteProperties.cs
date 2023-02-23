using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public interface IHasBaseWriteProperties
    {
        public BaseNotificationPropertiesWriteRequestDTO BaseProperties { get; set; }
    }
}
