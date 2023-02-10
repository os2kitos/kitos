using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.Notifications
{
    public class NotificationResponseDTO
    {
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
        /// Resource owning the notification
        /// </summary>
        public IdentityNamePairResponseDTO OwnerResource { get; set; }

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