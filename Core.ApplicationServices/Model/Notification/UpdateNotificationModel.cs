using Core.DomainModel.Advice;
using Core.DomainModel.Shared;
using System;

namespace Core.ApplicationServices.Model.Notification
{
    public class UpdateNotificationModel
    {
        public string Name { get; set; }
        public DateTime? ToDate { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public RelatedEntityType? Type { get; set; }
        public AdviceType AdviceType { get; set; }

        /// <summary>
        /// Id of the Owner Resource
        /// </summary>
        public int RelationId { get; set; }
    }
}
