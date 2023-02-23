using Core.DomainModel.Advice;
using System;
using Core.DomainModel;
using Core.DomainModel.Notification;

namespace Core.ApplicationServices.Model.Notification
{
    public class ScheduledNotificationModel : IHasBaseNotificationPropertiesModel, IHasRecipientModels, IHasName, IHasToDate
    {
        public string Name { get; set; }
        public DateTime? ToDate { get; set; }
        public Scheduling RepetitionFrequency { get; set; }
        public DateTime FromDate { get; set; }
        public BaseNotificationPropertiesModel BaseProperties { get; set; }
        public RecipientModel Ccs { get; set; }
        public RecipientModel Receivers{ get; set; }
    }
}
