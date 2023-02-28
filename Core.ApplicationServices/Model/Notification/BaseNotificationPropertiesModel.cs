using Core.DomainModel.Advice;
using Core.DomainModel.Shared;

namespace Core.ApplicationServices.Model.Notification
{
    public class BaseNotificationPropertiesModel
    {
        public BaseNotificationPropertiesModel(string subject, string body, RelatedEntityType type, AdviceType adviceType, int relationId)
        {
            Subject = subject;
            Body = body;
            Type = type;
            AdviceType = adviceType;
            RelationId = relationId;
        }

        public string Subject { get; }
        public string Body { get; }
        public RelatedEntityType Type { get; }
        public AdviceType AdviceType { get; }

        /// <summary>
        /// Id of the Owner Resource
        /// </summary>
        public int RelationId { get; }
    }
}
