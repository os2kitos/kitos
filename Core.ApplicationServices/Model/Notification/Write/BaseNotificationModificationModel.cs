using System;

namespace Core.ApplicationServices.Model.Notification.Write
{
    public abstract class BaseNotificationModificationModel
    {
        public string Name { get; set; }
        public DateTime? ToDate { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        /// <summary>
        /// Id of the Owner Resource
        /// </summary>
        public int? RelationId { get; set; }
    }
}
