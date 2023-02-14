using Core.DomainModel.Advice;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class NotificationModificationModel : BaseNotificationModificationModel
    {
        public Scheduling? RepetitionFrequency { get; set; }
        public DateTime? FromDate { get; set; }

        public IEnumerable<RecipientModificationModel> Recipients { get; set; }

    }
}
