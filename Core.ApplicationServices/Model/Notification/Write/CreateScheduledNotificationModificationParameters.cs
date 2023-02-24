using System;
using Core.DomainModel;
using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class CreateScheduledNotificationModificationParameters: IHasBaseNotificationPropertiesParameters, IHasReadonlyName, IHasReadonlyToDate
    {
        public CreateScheduledNotificationModificationParameters(BaseNotificationPropertiesModificationParameters baseProperties, string name, DateTime? toDate, Scheduling repetitionFrequency, DateTime fromDate)
        {
            BaseProperties = baseProperties;
            Name = name;
            ToDate = toDate;
            RepetitionFrequency = repetitionFrequency;
            FromDate = fromDate;
        }

        public BaseNotificationPropertiesModificationParameters BaseProperties { get; }
        public string Name { get; }
        public DateTime? ToDate { get; }
        public Scheduling RepetitionFrequency { get; }
        public DateTime FromDate { get; }

    }
}
