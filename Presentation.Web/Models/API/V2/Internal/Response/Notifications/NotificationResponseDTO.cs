using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Models.API.V2.Internal.Response.Notifications
{
    public class NotificationResponseDTO
    {
        /// <summary>
        /// Uuid of the Notification
        /// </summary>
        public Guid Uuid { get; set; }
        /// <summary>
        /// Indicates whether notification is active
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Name of the notification (different field than Subject)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Date when the notification was sent last time
        /// </summary>
        public DateTime? LastSent { get; set; }
        /// <summary>
        /// Date when the notification becomes active
        /// </summary>
        public DateTime? FromDate { get; set; }
        /// <summary>
        /// Date when the notification expires
        /// </summary>
        public DateTime? ToDate { get; set; }
        /// <summary>
        /// Subject of the email
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Body of the email
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// Resource owning the notification (e.g. ItContract)
        /// </summary>
        public OwnerResourceType OwnerResourceType { get; set; }
        /// <summary>
        /// Type of the notification (Immediate/Scheduled)
        /// </summary>
        public NotificationSendType NotificationType { get; set; }
        /// <summary>
        /// Resource owning the notification
        /// </summary>
        public IdentityNamePairResponseDTO OwnerResource { get; set; }
        /// <summary>
        /// Indicates how often should a scheduled notification be repeated
        /// </summary>
        public RepetitionFrequencyOptions? RepetitionFrequency{ get; set; }

        /// <summary>
        /// List of recipients
        /// </summary>
        public RecipientResponseDTO Receivers { get; set; }
        /// <summary>
        /// List of CCs
        /// </summary>
        public RecipientResponseDTO CCs { get; set; }
    }
}