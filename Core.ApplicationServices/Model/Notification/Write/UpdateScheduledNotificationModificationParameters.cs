using System;
using Core.DomainModel;
using Core.DomainModel.Notification;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class UpdateScheduledNotificationModificationParameters: IHasBaseNotificationPropertiesParameters, IHasReadonlyName, IHasReadonlyToDate
    {
        public UpdateScheduledNotificationModificationParameters(BaseNotificationPropertiesModificationParameters baseProperties, string name, DateTime? toDate)
        {
            BaseProperties = baseProperties;
            Name = name;
            ToDate = toDate;
        }

        public BaseNotificationPropertiesModificationParameters BaseProperties { get; }
        public string Name { get; }
        public DateTime? ToDate { get; }

    }
}
