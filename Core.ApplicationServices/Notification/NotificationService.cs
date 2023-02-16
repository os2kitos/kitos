using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel.Advice;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IRegistrationNotificationService _registrationNotificationService;
        private readonly IRegistrationNotificationUserRelationsService _notificationUserRelationsService;
        private readonly IAuthorizationContext _authorizationContext;

        public NotificationService(IEntityIdentityResolver entityIdentityResolver,
            IRegistrationNotificationService registrationNotificationService, 
            IAuthorizationContext authorizationContext,
            IRegistrationNotificationUserRelationsService notificationUserRelationsService)
        {
            _entityIdentityResolver = entityIdentityResolver;
            _registrationNotificationService = registrationNotificationService;
            _authorizationContext = authorizationContext;
            _notificationUserRelationsService = notificationUserRelationsService;
        }

        public Result<IQueryable<Advice>, OperationError> GetNotifications(Guid organizationUuid, params IDomainQuery<Advice>[] conditions)
        {
            return ResolveOrganizationId(organizationUuid)
                .Bind(orgId =>
                {
                    var getResult = _registrationNotificationService.GetNotificationsByOrganizationId(orgId);
                    if (getResult.Failed)
                        return getResult.Error;
                    var baseQuery = getResult.Value;
                    var subQueries = new List<IDomainQuery<Advice>>();
                    subQueries.AddRange(conditions);

                    var result = subQueries.Any()
                        ? new IntersectionQuery<Advice>(subQueries).Apply(baseQuery)
                        : baseQuery;

                    return Result<IQueryable<Advice>, OperationError>.Success(result);
                });
        }

        public Result<Advice, OperationError> GetNotificationByUuid(Guid uuid, RelatedEntityType relatedEntityType)
        {
            return _entityIdentityResolver.ResolveDbId<Advice>(uuid)
                .Match(id => _registrationNotificationService.GetNotificationById(id)
                        .Match<Result<Advice, OperationError>>
                        (
                            notification => notification, 
                            () => new OperationError($"Notification with Id: {id} was not found", OperationFailure.NotFound)
                        ), 
                    () => new OperationError($"Id for notification with uuid: {uuid} was not found", OperationFailure.NotFound))
                .Match<Result<Advice,OperationError>>(notification =>
                {
                    if (_authorizationContext.AllowReads(notification))
                        return new OperationError("User is not allowed to read notifications", OperationFailure.Forbidden);
                    if(notification.Type != relatedEntityType)
                        return new OperationError($"Notification related entity type is different than {relatedEntityType}", OperationFailure.BadInput);

                    return notification;
                }, error => error);
        }

        public IEnumerable<AdviceSent> GetNotificationSentByUuid(Guid uuid, RelatedEntityType relatedEntityType)
        {
            return _registrationNotificationService.GetSent()
                .Where(x => x.Advice.Uuid == uuid && x.Advice.Type == relatedEntityType).ToList();
        }

        public Result<Advice, OperationError> CreateImmediateNotification(Guid organizationUuid, ImmediateNotificationModificationParameters parameters)
        {
            return MapModelAndResolveOrgId(organizationUuid, parameters)
                .Bind(result => _registrationNotificationService.Create(result.orgId, result.model));
        }

        public Result<Advice, OperationError> CreateScheduledNotification(Guid organizationUuid, ScheduledNotificationModificationParameters parameters)
        {
            return MapModelAndResolveOrgId(organizationUuid, parameters)
                .Bind(result =>
                {
                    var model = result.model;
                    model.FromDate = parameters.FromDate;
                    model.ToDate = parameters.ToDate;
                    model.RepetitionFrequency = parameters.RepetitionFrequency;
                    model.Name = parameters.Name;

                    return _registrationNotificationService.Create(result.orgId, model);
                });
        }

        public Result<Advice, OperationError> UpdateScheduledNotification(Guid notificationUuid, UpdateScheduledNotificationModificationParameters parameters)
        {
            return MapBaseModel(parameters)
                .Bind(model =>
                {
                    model.ToDate = parameters.ToDate;
                    model.Name = parameters.Name;

                    var recipients = model.Recipients;
                    return _entityIdentityResolver.ResolveDbId<Advice>(notificationUuid)
                        .Match<Result<int, OperationError>>(notificationId =>
                            {
                                var result =
                                    _notificationUserRelationsService.UpdateNotificationUserRelations(notificationId,
                                        recipients);
                                if (result.HasValue)
                                    return result.Value;

                                return notificationId;
                            },
                            () => new OperationError($"Id for notification with uuid: {notificationUuid} was not found",
                                OperationFailure.NotFound)
                        )
                        .Bind(notificationId => _registrationNotificationService.Update(notificationId, model));
                });
        }

        public Maybe<OperationError> DeleteNotification(Guid notificationUuid, RelatedEntityType relatedEntityType)
        {
            return GetNotificationByUuid(notificationUuid, relatedEntityType)
                .Match(notification => _authorizationContext.AllowDelete(notification)
                    ? _registrationNotificationService.Delete(notification.Id)
                    : new OperationError($"User is not allowed to delete notification with uuid: {notificationUuid}", OperationFailure.Forbidden),
                    error => error);
        }

        private Result<(int orgId, NotificationModel model), OperationError> MapModelAndResolveOrgId(Guid organizationUuid, ImmediateNotificationModificationParameters parameters)
        {
            return MapBaseModel(parameters)
                .Bind(model =>
                {
                    return ResolveOrganizationId(organizationUuid).Select(id => (id, model));
                });
        }

        private Result<int, OperationError> ResolveOrganizationId(Guid organizationUuid)
        {
            return _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid)
                .Match<Result<int, OperationError>>
                (
                    id => id,
                    () => new OperationError($"Id for Organization with uuid: {organizationUuid} was not found",
                        OperationFailure.NotFound)
                );
        }

        private Result<NotificationModel, OperationError> MapBaseModel(ImmediateNotificationModificationParameters parameters)
        {
            var model = new NotificationModel
            {
                AdviceType = AdviceType.Immediate,
                Body = parameters.Body,
                Type = parameters.Type,
            };
            return ResolveRelationId(parameters.OwnerResourceUuid, parameters.Type)
                .Match
                (
                    relationId =>
                    {
                        model.RelationId = relationId;
                        return MapRecipients(parameters);
                    },
                    () => new OperationError(
                        $"Id for owner type with uuid: {parameters.OwnerResourceUuid} was not found",
                        OperationFailure.NotFound)
                )
                .Match<Result<NotificationModel, OperationError>>(recipients =>
                {
                    model.Recipients = recipients;
                    return model;
                }, error => error);
        }

        private Result<IEnumerable<RecipientModel>, OperationError> MapRecipients(
            ImmediateNotificationModificationParameters parameters)
        {
            return MapRootRecipientModel(parameters.Ccs, RecieverType.CC, parameters.Type)
                .Bind(ccRecipients => 
                    MapRootRecipientModel(parameters.Receivers, RecieverType.RECIEVER, parameters.Type)
                    .Bind<IEnumerable<RecipientModel>>(receiverRecipients => 
                        ccRecipients.Concat(receiverRecipients).ToList()));
        }

        private Result<IEnumerable<RecipientModel>, OperationError> MapRootRecipientModel(
            RootRecipientModificationParameters root, RecieverType receiverType, RelatedEntityType relatedEntityType)
        {
            return MapRoleRecipients(root.RoleRecipients, receiverType, relatedEntityType)
                .Bind<IEnumerable<RecipientModel>>(roleRecipients => 
                    MapEmailRecipients(root.EmailRecipients, receiverType).Concat(roleRecipients).ToList());
        }

        private static IEnumerable<RecipientModel> MapEmailRecipients(IEnumerable<EmailRecipientModificationParameters> emailRecipients, RecieverType receiverType)
        {
            return emailRecipients.Select(x => new RecipientModel
            {
                Email = x.Email,
                RecipientType = RecipientType.USER,
                ReceiverType = receiverType
            });
        }

        private Result<IEnumerable<RecipientModel>, OperationError> MapRoleRecipients(IEnumerable<RoleRecipientModificationParameters> roleRecipients, RecieverType receiverType, RelatedEntityType relatedEntityType)
        {
            var recipients = new List<RecipientModel>();
            foreach (var recipient in roleRecipients)
            {
                var newRecipient = new RecipientModel{RecipientType = RecipientType.ROLE, ReceiverType = receiverType};
                var error = ResolveRoleIdAndAssignToRoleRecipient(recipient.RoleUuid, relatedEntityType, newRecipient);
                if (error.HasValue)
                    return error.Value;

                recipients.Add(newRecipient);
            }

            return recipients;
        }

        private Maybe<OperationError> ResolveRoleIdAndAssignToRoleRecipient(Guid roleUuid,
            RelatedEntityType relatedEntityType, RecipientModel recipientModel)
        {
            var roleIdResult = ResolveRelationId(roleUuid, relatedEntityType);
            if(roleIdResult.IsNone)
                return new OperationError($"Id for {nameof(RelatedEntityType)} with uuid: {roleUuid} was not found", OperationFailure.NotFound);
            var roleId = roleIdResult.Value;

            switch(relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    recipientModel.DataProcessingRegistrationRoleId = roleId;
                    break;
                case RelatedEntityType.itContract:
                    recipientModel.ItContractRoleId = roleId;
                    break;
                case RelatedEntityType.itSystemUsage:
                    recipientModel.ItSystemRoleId = roleId;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null);
            };

            return Maybe<OperationError>.None;
        }

        private Maybe<int> ResolveRelationId(Guid relationUuid, RelatedEntityType relatedEntityType)
        {
            return relatedEntityType switch
            {
                RelatedEntityType.dataProcessingRegistration => _entityIdentityResolver.ResolveDbId<DataProcessingRegistration>(relationUuid),
                RelatedEntityType.itContract => _entityIdentityResolver.ResolveDbId<ItContract>(relationUuid),
                RelatedEntityType.itSystemUsage => _entityIdentityResolver.ResolveDbId<ItSystemUsage>(relationUuid),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null)
            };
        }
    }
}
