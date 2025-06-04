using Core.Abstractions.Types;
using Core.DomainModel.Advice;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public static class NotificationTypeMappingExtensions
    {
        private static readonly EnumMap<NotificationSendType, AdviceType> Mapping;

        static NotificationTypeMappingExtensions()
        {
            Mapping = new EnumMap<NotificationSendType, AdviceType>
            (
                (NotificationSendType.Immediate, AdviceType.Immediate),
                (NotificationSendType.Repeat, AdviceType.Repeat)
            );
        }

        public static NotificationSendType ToNotificationType(this AdviceType value)
        {
            return Mapping.FromRightToLeft(value);
        }
    }
}