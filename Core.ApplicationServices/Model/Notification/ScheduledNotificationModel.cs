using Core.DomainModel.Advice;
using System;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel;

namespace Core.ApplicationServices.Model.Notification
{
    public class ScheduledNotificationModel : IHasBaseNotificationPropertiesModel, IHasRecipientModels, IHasReadonlyName, IHasReadonlyToDate
    {
        public ScheduledNotificationModel(string name, DateTime? toDate, Scheduling repetitionFrequency, DateTime fromDate, BaseNotificationPropertiesModel baseProperties, RecipientModel ccs, RecipientModel receivers)
        {
            Name = name;
            ToDate = toDate;
            RepetitionFrequency = repetitionFrequency;
            FromDate = fromDate;
            BaseProperties = baseProperties;
            Ccs = ccs;
            Receivers = receivers;
        }

        public string Name { get; }
        public DateTime? ToDate { get; }
        public Scheduling RepetitionFrequency { get; }
        public DateTime FromDate { get; }
        public BaseNotificationPropertiesModel BaseProperties { get; }
        public RecipientModel Ccs { get; }
        public RecipientModel Receivers{ get; }
    }
}
