using Core.DomainModel.Advice;
using System;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public class ScheduledNotificationModificationParameters : ImmediateNotificationModificationParameters
    {
        public OptionalValueChange<Scheduling> RepetitionFrequency { get; set; }
        public OptionalValueChange<DateTime> FromDate { get; set; }
    }
}
