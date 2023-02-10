using System;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public abstract class BaseScheduledNotificationWriteRequestDTO : ImmediateNotificationWriteRequestDTO
    {
        /// <summary>
        /// Name of the notification (don't confuse it with the Subject)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Date on which the notification expires
        /// </summary>
        public DateTime ToDate { get; set; }
    }
}