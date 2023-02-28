using System;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel;

namespace Core.ApplicationServices.Model.Notification
{
    public class UpdateScheduledNotificationModel: IHasBaseNotificationPropertiesModel, IHasReadonlyName, IHasReadonlyToDate
    {
        public UpdateScheduledNotificationModel(string name, DateTime? toDate, BaseNotificationPropertiesModel baseProperties)
        {
            Name = name;
            ToDate = toDate;
            BaseProperties = baseProperties;
        }

        public string Name { get; }
        public DateTime? ToDate { get; }
        public BaseNotificationPropertiesModel BaseProperties { get; }
    }
}
