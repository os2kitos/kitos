using Core.DomainModel.Advice;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.Notification
{
    public class NotificationModel : UpdateNotificationModel
    {
        public Scheduling? RepetitionFrequency { get; set; }
        public DateTime? FromDate { get; set; }

        public IEnumerable<RecipientModel> Recipients { get; set; }

    }
}
