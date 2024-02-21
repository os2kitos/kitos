using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Read;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public class NotificationResponseMapper : INotificationResponseMapper
    {
        public NotificationResponseDTO MapNotificationResponseDTO(NotificationResultModel notification)
        {
            var relatedEntityType = notification.RelatedEntityType.ToOwnerResourceType();
            return new NotificationResponseDTO
            {
                Active = notification.IsActive,
                Name = notification.Name,
                LastSent = notification.SentDate,
                FromDate = notification.FromDate,
                ToDate = notification.ToDate,
                Subject = notification.Subject,
                Body = notification.Body,
                RepetitionFrequency = notification.RepetitionFrequency?.ToRepetitionFrequencyOptions(),
                OwnerResourceType = relatedEntityType,
                NotificationType = notification.NotificationType.ToNotificationType(),
                Receivers = MapRecipientResponseDTO(notification.Receivers),
                CCs = MapRecipientResponseDTO(notification.Ccs),
                OwnerResource = MapIEntityWithAdvicesToResponseDTO(notification),
                Uuid = notification.Uuid
            };
        }

        public NotificationResourcePermissionsDTO MapNotificationAccessRightsResponseDTO(NotificationPermissions permissions)
        {
            return new NotificationResourcePermissionsDTO
            {
                Read = permissions.Read,
                Modify = permissions.Modify,
                Deactivate = permissions.Deactivate,
                Delete = permissions.Delete,
            };
        }

        public NotificationSentResponseDTO MapNotificationSentResponseDTO(AdviceSent notificationSent)
        {
            return new NotificationSentResponseDTO
            {
                SentDate = notificationSent.AdviceSentDate
            };
        }

        private static RecipientResponseDTO MapRecipientResponseDTO(RecipientResultModel recipients)
        {
            return new RecipientResponseDTO
            {
                EmailRecipients = recipients?.EmailRecipients != null ? MapEmailRecipientResponseDTOs(recipients.EmailRecipients) : null,
                RoleRecipients = recipients?.RoleRecipients != null ? MapRoleRecipientResponseDTOs(recipients.RoleRecipients) : null
            };
        }

        private static IEnumerable<EmailRecipientResponseDTO> MapEmailRecipientResponseDTOs(IEnumerable<EmailRecipientResultModel> recipients)
        {
            return recipients.Select(MapEmailRecipientResponseDTO).ToList();
        }

        private static EmailRecipientResponseDTO MapEmailRecipientResponseDTO(EmailRecipientResultModel notification)
        {
            return new EmailRecipientResponseDTO
            {
                Email = notification.Email
            };
        }

        private static IEnumerable<RoleRecipientResponseDTO> MapRoleRecipientResponseDTOs(IEnumerable<RoleRecipientResultModel> recipients)
        {
            return recipients.Select(MapRoleRecipientResponseDTO).ToList();
        }

        private static RoleRecipientResponseDTO MapRoleRecipientResponseDTO(RoleRecipientResultModel recipient)
        {
            return new RoleRecipientResponseDTO
            {
                Role = recipient.Role.MapIdentityNamePairDTO()
            };
        }

        private static IdentityNamePairResponseDTO MapIEntityWithAdvicesToResponseDTO(NotificationResultModel notification)
        {
            return notification.RelatedEntityType switch
            {
                RelatedEntityType.dataProcessingRegistration =>
                    ((DataProcessingRegistration) notification.OwnerResource)?.MapIdentityNamePairDTO(),
                RelatedEntityType.itContract => ((ItContract) notification.OwnerResource)?.MapIdentityNamePairDTO(),
                RelatedEntityType.itSystemUsage => ((ItSystemUsage)notification.OwnerResource)?.MapIdentityNamePairDTO(),
                _ => throw new ArgumentOutOfRangeException(nameof(notification.RelatedEntityType), notification.RelatedEntityType,
                    $"NotificationResponseMapper:MapIEntityWithAdvicesToResponseDTO doesn't support value: {notification.RelatedEntityType}")
            };
        }
    }
}