using Core.DomainModel.Advice;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Model.Notification
{
    public class BaseNotificationPropertiesModel
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public RelatedEntityType Type { get; set; }
        public AdviceType AdviceType { get; set; }

        /// <summary>
        /// Id of the Owner Resource
        /// </summary>
        public int RelationId { get; set; }
    }
}
