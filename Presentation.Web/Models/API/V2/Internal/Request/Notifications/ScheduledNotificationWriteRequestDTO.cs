using System;
using System.ComponentModel.DataAnnotations;
using Core.DomainModel;
using Core.DomainModel.Notification;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Models.API.V2.Internal.Request.Notifications
{
    public class ScheduledNotificationWriteRequestDTO: IHasBaseWriteProperties, IHasName, IHasToDate
    {
        /// <summary>
        /// Name of the notification (different from the Subject)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Date on which the notification expires
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Indicates how often should the notification be repeated
        /// </summary>
        [Required]
        public RepetitionFrequencyOptions RepetitionFrequency { get; set; }
        /// <summary>
        /// Date from which the notification is active
        /// </summary>
        [Required]
        public DateTime FromDate { get; set; }
        
        [Required]
        public BaseNotificationPropertiesWriteRequestDTO BaseProperties { get; set; }
    }
}