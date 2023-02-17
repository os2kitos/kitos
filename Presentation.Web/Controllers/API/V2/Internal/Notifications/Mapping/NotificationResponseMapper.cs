using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Notification;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Core.DomainServices.Generic;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Presentation.Web.Models.API.V2.Internal.Response.Notifications;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public class NotificationResponseMapper : INotificationResponseMapper
    {
        private readonly IEntityIdentityResolver _identityResolver;

        public NotificationResponseMapper(IEntityIdentityResolver identityResolver)
        {
            _identityResolver = identityResolver;
        }

        public Result<NotificationResponseDTO, OperationError> MapNotificationResponseDTO(Advice notification)
        {
            var type = notification.Type;
            var ownerResourceUuidResult = ResolveRelationId(notification.RelationId.GetValueOrDefault(), type);
            if(ownerResourceUuidResult.IsNone)
                return new OperationError($"Uuid was not found for relation with id: {notification.RelationId}", OperationFailure.NotFound);

            return new NotificationResponseDTO
            {
                Active = notification.IsActive,
                Name = notification.Name,
                LastSent = notification.SentDate,
                FromDate = notification.AlarmDate,
                ToDate = notification.StopDate,
                Subject = notification.Subject,
                Body = notification.Body,
                RepetitionFrequency = notification.Scheduling?.ToRepetitionFrequencyOptions(),
                Type = type.ToOwnerResourceType(),
                NotificationType = notification.AdviceType.ToNotificationType(),
                Receivers = MapRecipients(notification, RecieverType.RECIEVER, type),
                CCs = MapRecipients(notification, RecieverType.CC, type),
                OwnerResourceUuid = ownerResourceUuidResult.Value,
                Uuid = notification.Uuid
            };
        }

        public Result<IEnumerable<NotificationResponseDTO>, OperationError> MapNotificationResponseDTOs(IEnumerable<Advice> notifications)
        {
            var result = new List<NotificationResponseDTO>();
            foreach (var notification in notifications)
            {
                var mappedNotification = MapNotificationResponseDTO(notification);
                if (mappedNotification.Failed)
                    return mappedNotification.Error;
                result.Add(mappedNotification.Value);
            }

            return result;
        }

        public NotificationAccessRightsResponseDTO MapNotificationAccessRightsResponseDTO(NotificationAccessRights accessRights)
        {
            return new NotificationAccessRightsResponseDTO
            {
                CanBeDeactivated = accessRights.CanBeDeactivated,
                CanBeDeleted = accessRights.CanBeDeleted,
                CanBeModified = accessRights.CanBeModified,
            };
        }

        public NotificationSentResponseDTO MapNotificationSentResponseDTO(AdviceSent notificationSent)
        {
            return new NotificationSentResponseDTO
            {
                SentDate = notificationSent.AdviceSentDate
            };
        }

        private static RecipientResponseDTO MapRecipients(Advice notification, RecieverType receiverType, RelatedEntityType relatedEntityType)
        {
            var recipientsByType = notification.Reciepients.Where(x => x.RecieverType == receiverType).ToList();
            return MapRecipientResponseDTO(recipientsByType, relatedEntityType);
        }

        private static RecipientResponseDTO MapRecipientResponseDTO(IEnumerable<AdviceUserRelation> recipients, RelatedEntityType relatedEntityType)
        {
            var recipientList = recipients.ToList();
            if (recipientList.Any())
            {
                var emailRecipients = recipientList.Where(x => x.RecpientType == RecipientType.USER).ToList();
                var roleRecipients = recipientList.Where(x => x.RecpientType == RecipientType.ROLE).ToList();

                var recipient = new RecipientResponseDTO
                {
                    EmailRecipients = emailRecipients.Any() ? MapEmailRecipientResponseDTOs(emailRecipients) : null,
                    RoleRecipients = roleRecipients.Any() ? MapRoleRecipientResponseDTOs(roleRecipients, relatedEntityType) : null
                };
                return recipient;
            }

            return null;
        }

        private static IEnumerable<EmailRecipientResponseDTO> MapEmailRecipientResponseDTOs(IEnumerable<AdviceUserRelation> recipients)
        {
            return recipients.Select(MapEmailRecipientResponseDTO);
        }

        private static EmailRecipientResponseDTO MapEmailRecipientResponseDTO(AdviceUserRelation notification)
        {
            return new EmailRecipientResponseDTO
            {
                Email = notification.Email
            };
        }

        private static IEnumerable<RoleRecipientResponseDTO> MapRoleRecipientResponseDTOs(IEnumerable<AdviceUserRelation> recipients, RelatedEntityType relatedEntityType)
        {
            return recipients.Select(x => MapRoleRecipientResponseDTO(x, relatedEntityType));
        }

        private static RoleRecipientResponseDTO MapRoleRecipientResponseDTO(AdviceUserRelation notificationUserRelation, RelatedEntityType relatedEntityType)
        {
            return new RoleRecipientResponseDTO
            {
                Role = GetRoleIdentityNamePair(notificationUserRelation, relatedEntityType)
            };
        }

        private static IdentityNamePairResponseDTO GetRoleIdentityNamePair(AdviceUserRelation userRelation, RelatedEntityType relatedEntityType)
        {

            return relatedEntityType switch
            {
                RelatedEntityType.dataProcessingRegistration => userRelation.DataProcessingRegistrationRole.MapIdentityNamePairDTO(),
                RelatedEntityType.itContract => userRelation.ItContractRole.MapIdentityNamePairDTO(),
                RelatedEntityType.itSystemUsage => userRelation.ItSystemRole.MapIdentityNamePairDTO(),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null)
            };
        }

        private Maybe<Guid> ResolveRelationId(int relationId, RelatedEntityType relatedEntityType)
        {
            return relatedEntityType switch
            {
                RelatedEntityType.dataProcessingRegistration => _identityResolver.ResolveUuid<DataProcessingRegistration>(relationId),
                RelatedEntityType.itContract => _identityResolver.ResolveUuid<ItContract>(relationId),
                RelatedEntityType.itSystemUsage => _identityResolver.ResolveUuid<ItSystemUsage>(relationId),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null)
            };
        }
    }
}