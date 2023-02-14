using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class UpdateNotificationModificationModel : BaseNotificationModificationModel
    {
        public IEnumerable<UpdateRecipientModificationModel> Recipients { get; set; }
    }
}
