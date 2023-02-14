using System;
using System.Collections.Generic;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Request.Notifications;
using Core.Abstractions.Types;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Generic;
using Presentation.Web.Models.API.V2.Types.Notifications;

namespace Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping
{
    public class NotificationWriteModelMapper : INotificationWriteModelMapper
    {
        private readonly IEntityIdentityResolver _entityIdentityResolver;

        public NotificationWriteModelMapper(IEntityIdentityResolver entityIdentityResolver)
        {
            _entityIdentityResolver = entityIdentityResolver;
        }

        public Result<Advice, OperationError> FromImmediatePOST(ImmediateNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return MapImmediateNotificationWriteRequestDTO(dto, ownerResourceType);
        }

        public Result<Advice, OperationError> FromScheduledPOST(ScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return MapScheduledNotificationWriteRequestDTO(dto, ownerResourceType);
        }

        public Result<Advice, OperationError> FromScheduledPUT(UpdateScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return MapBaseScheduledNotificationWriteRequestDTO(dto, ownerResourceType);
        }

        private Result<Advice, OperationError> MapImmediateNotificationWriteRequestDTO(ImmediateNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            var advice = new Advice
            {
                Body = dto.Body,
                Subject = dto.Subject,
                Type = ownerResourceType.ToRelatedEntityType()
            };
            return ResolveIdForOwnerResource(dto.OwnerResource.Uuid, ownerResourceType)
                .Bind(ownerResourceId =>
                {
                    advice.RelationId = ownerResourceId;
                    return MapAllRecipients(dto, ownerResourceType);
                })
                .Bind<Advice>(recipients =>
                {
                    advice.Reciepients = recipients.ToList();
                    return advice;
                });
        }

        private Result<Advice, OperationError> MapBaseScheduledNotificationWriteRequestDTO(BaseScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return MapImmediateNotificationWriteRequestDTO(dto, ownerResourceType)
                .Select(advice =>
                {
                    advice.Name = dto.Name;
                    advice.StopDate = dto.ToDate;

                    return advice;
                });
        }

        private Result<Advice, OperationError> MapScheduledNotificationWriteRequestDTO(ScheduledNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return MapBaseScheduledNotificationWriteRequestDTO(dto, ownerResourceType)
                .Select(advice =>
                {
                    advice.Scheduling = dto.RepetitionFrequency.ToScheduling();
                    advice.AlarmDate = dto.FromDate;

                    return advice;
                });
        }

        private Result<IEnumerable<AdviceUserRelation>, OperationError> MapAllRecipients(ImmediateNotificationWriteRequestDTO dto, OwnerResourceType ownerResourceType)
        {
            return MapEmailAndRoleRecipients(dto.Ccs, ownerResourceType, RecieverType.CC)
                .Bind(recipients =>
                {
                    var recipientList = (recipients ?? new List<AdviceUserRelation>()).ToList();
                    return MapEmailAndRoleRecipients(dto.Receivers, ownerResourceType, RecieverType.RECIEVER)
                        .Bind(receiverRecipients =>
                        {
                            recipientList.AddRange(receiverRecipients);
                            return Result<IEnumerable<AdviceUserRelation>, OperationError>.Success(recipientList);
                        });
                });
        }

        private Result<IEnumerable<AdviceUserRelation>, OperationError> MapEmailAndRoleRecipients(RecipientWriteRequestDTO dto, OwnerResourceType ownerResourceType, RecieverType receiverType)
        {
            return MapRoleRecipients(dto.RoleRecipients, ownerResourceType, receiverType)
                .Bind<IEnumerable<AdviceUserRelation>>(roleRecipients =>
                {
                    var recipientList = (roleRecipients ?? new List<AdviceUserRelation>()).ToList();
                    recipientList.AddRange(MapEmailRecipients(dto.EmailRecipients, receiverType));
                    return recipientList;
                });
        }

        private static IEnumerable<AdviceUserRelation> MapEmailRecipients(IEnumerable<EmailRecipientWriteRequestDTO> dto, RecieverType receiverType)
        {
            return dto.Select(x => new AdviceUserRelation
            {
                Email = x.Email,
                RecieverType = receiverType,
                RecpientType = RecipientType.USER
            }).ToList();
        }

        private Result<IEnumerable<AdviceUserRelation>, OperationError> MapRoleRecipients(IEnumerable<RoleRecipientWriteRequestDTO> dtos, OwnerResourceType ownerResourceType, RecieverType receiverType)
        {
            var recipients = new List<AdviceUserRelation>();
            foreach (var dto in dtos)
            {
                var recipientResult = MapRoleRecipient(dto, ownerResourceType, receiverType);
                if (recipientResult.Failed)
                    return recipientResult.Error;

                recipients.Add(recipientResult.Value);
            }

            return recipients;
        }

        private Result<AdviceUserRelation, OperationError> MapRoleRecipient(RoleRecipientWriteRequestDTO dto,
            OwnerResourceType ownerResourceType, RecieverType receiverType)
        {
            var notificationRelation = new AdviceUserRelation
            {
                RecieverType = receiverType,
                RecpientType = RecipientType.ROLE
            };
            return AssignRoleId(dto.RoleUuid, ownerResourceType, notificationRelation)
                .Match<Result<AdviceUserRelation, OperationError>>(error => error, () => notificationRelation);
        }

        private Maybe<OperationError> AssignRoleId(Guid uuid, OwnerResourceType ownerResourceType, AdviceUserRelation notificationRelation)
        {
            var ownerIdResult = ResolveIdForOwnerResource(uuid, ownerResourceType);
            if (ownerIdResult.Failed)
                return ownerIdResult.Error;
            var ownerId = ownerIdResult.Value;

            switch(ownerResourceType)
            {
                case OwnerResourceType.DataProcessingRegistration: notificationRelation.DataProcessingRegistrationRoleId = ownerId; break;
                case OwnerResourceType.ItContract: notificationRelation.ItContractRoleId = ownerId; break;
                case OwnerResourceType.ItSystemUsage: notificationRelation.ItSystemRoleId = ownerId; break;
                default: throw new ArgumentOutOfRangeException(nameof(ownerResourceType), ownerResourceType, null);
            }

            return Maybe<OperationError>.None;
        }

        private Result<int, OperationError> ResolveIdForOwnerResource(Guid uuid, OwnerResourceType ownerResourceType)
        {
            return ownerResourceType switch
            {
                OwnerResourceType.DataProcessingRegistration => _entityIdentityResolver
                    .ResolveDbId<DataProcessingRegistration>(uuid)
                    .Match<Result<int, OperationError>>
                    (
                        id => id,
                        () => CreateIdNotFoundOperationError(uuid, nameof(DataProcessingRegistration))
                    ),
                OwnerResourceType.ItContract => _entityIdentityResolver.ResolveDbId<ItContract>(uuid)
                    .Match<Result<int, OperationError>>
                    (
                        id => id,
                        () => CreateIdNotFoundOperationError(uuid, nameof(ItContract))
                    ),
                OwnerResourceType.ItSystemUsage => _entityIdentityResolver.ResolveDbId<ItSystemUsage>(uuid)
                    .Match<Result<int, OperationError>>
                    (
                        id => id,
                        () => CreateIdNotFoundOperationError(uuid, nameof(ItSystemUsage))
                    ),
                _ => throw new ArgumentOutOfRangeException(nameof(ownerResourceType), ownerResourceType, null)
            };
        }

        private static OperationError CreateIdNotFoundOperationError(Guid uuid, string typeName)
        {
            return new OperationError($"Id for {typeName} with uuid: {uuid} was not found", OperationFailure.NotFound);
        }
    }
}