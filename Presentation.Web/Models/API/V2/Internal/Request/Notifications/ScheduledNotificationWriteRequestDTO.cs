using System;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class ScheduledNotificationWriteRequestDTO : UpdateScheduledNotificationWriteRequestDTO
    {
        /// <summary>
        /// Indicates how often should the notification be repeated
        /// </summary>
        public RepetitionFrequencyOptions RepetitionFrequency { get; set; }
        /// <summary>
        /// Date from which the notification is active
        /// </summary>
        public DateTime FromDate { get; set; }
    }
}