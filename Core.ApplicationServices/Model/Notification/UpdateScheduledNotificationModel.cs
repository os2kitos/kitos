using System;
using Core.DomainModel;
using Core.DomainModel.Notification;

namespace Core.ApplicationServices.Model.Notification
{
    public class UpdateScheduledNotificationModel: IHasBaseNotificationPropertiesModel, IHasName, IHasToDate
    {
        public string Name { get; set; }
        public DateTime? ToDate { get; set; }
        public BaseNotificationPropertiesModel BaseProperties { get; set; }
    }
}
