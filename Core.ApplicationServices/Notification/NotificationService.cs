using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IRegistrationNotificationService _registrationNotificationService;
        private readonly IRegistrationNotificationUserRelationsService _notificationUserRelationsService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<ItSystemUsage> _usageRepository;
        private readonly IGenericRepository<ItContract> _contractRepository;
        private readonly IGenericRepository<DataProcessingRegistration> _dprRepository;
        private readonly IDomainEvents _domainEvents;

        public NotificationService(IEntityIdentityResolver entityIdentityResolver,
            IRegistrationNotificationService registrationNotificationService, 
            IAuthorizationContext authorizationContext,
            IRegistrationNotificationUserRelationsService notificationUserRelationsService, 
            ITransactionManager transactionManager,
            IGenericRepository<ItSystemUsage> usageRepository, 
            IGenericRepository<ItContract> contractRepository, 
            IGenericRepository<DataProcessingRegistration> dprRepository,
            IDomainEvents domainEvents)
        {
            _entityIdentityResolver = entityIdentityResolver;
            _registrationNotificationService = registrationNotificationService;
            _authorizationContext = authorizationContext;
            _notificationUserRelationsService = notificationUserRelationsService;
            _transactionManager = transactionManager;
            _usageRepository = usageRepository;
            _contractRepository = contractRepository;
            _dprRepository = dprRepository;
            _domainEvents = domainEvents;
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
                    var test = baseQuery.ToList();
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

        public Result<Advice, OperationError> CreateImmediateNotification(ImmediateNotificationModificationParameters parameters)
        {
            return Modify(parameters.OwnerResourceUuid, parameters.Type, relatedEntity =>
            {
               return MapBaseModel(parameters, AdviceType.Immediate, relatedEntity.Id)
                    .Bind(result => _registrationNotificationService.Create(result));
            });
        }

        public Result<Advice, OperationError> CreateScheduledNotification(ScheduledNotificationModificationParameters parameters)
        {
            return Modify(parameters.OwnerResourceUuid, parameters.Type, relatedEntity =>
            {
                return MapBaseModel(parameters, AdviceType.Repeat, relatedEntity.Id)
                    .Bind(result =>
                    {
                        result.FromDate = parameters.FromDate;
                        result.ToDate = parameters.ToDate;
                        result.RepetitionFrequency = parameters.RepetitionFrequency;
                        result.Name = parameters.Name;

                        return _registrationNotificationService.Create(result);
                    });
            });
        }

        public Result<Advice, OperationError> UpdateScheduledNotification(Guid notificationUuid, UpdateScheduledNotificationModificationParameters parameters)
        {
            return Modify(parameters.OwnerResourceUuid, parameters.Type, relatedEntity =>
            {
                return MapBaseModel(parameters, AdviceType.Repeat, relatedEntity.Id)
                    .Bind(model =>
                    {
                        model.ToDate = parameters.ToDate;
                        model.Name = parameters.Name;

                        var recipients = model.Recipients;
                        return _entityIdentityResolver.ResolveDbId<Advice>(notificationUuid)
                            .Match<Result<int, OperationError>>(notificationId =>
                                {
                                    var result = _notificationUserRelationsService.UpdateNotificationUserRelations(notificationId, recipients);
                                    if (result.HasValue)
                                        return result.Value;

                                    return notificationId;
                                },
                                () => new OperationError($"Id for notification with uuid: {notificationUuid} was not found", OperationFailure.NotFound)
                            )
                            .Bind(notificationId => _registrationNotificationService.Update(notificationId, model));
                    });
            });
        }

        public Result<Advice, OperationError> DeactivateNotification(Guid notificationUuid, RelatedEntityType relatedEntityType)
        {
            return GetNotificationByUuid(notificationUuid, relatedEntityType)
                .Match(notification => _authorizationContext.AllowModify(notification)
                        ? _registrationNotificationService.DeactivateNotification(notification.Id)
                        : new OperationError($"User is not allowed to deactivate notification with uuid: {notificationUuid}", OperationFailure.Forbidden),
                    error => error);
        }

        public Maybe<OperationError> DeleteNotification(Guid notificationUuid, RelatedEntityType relatedEntityType)
        {
            return GetNotificationByUuid(notificationUuid, relatedEntityType)
                .Match(notification => _authorizationContext.AllowDelete(notification)
                    ? _registrationNotificationService.Delete(notification.Id)
                    : new OperationError($"User is not allowed to delete notification with uuid: {notificationUuid}", OperationFailure.Forbidden),
                    error => error);
        }

        public Result<NotificationAccessRights, OperationError> GetAccessRights(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            var notificationResult = GetNotificationByUuid(notificationUuid, relatedEntityType);
            if (notificationResult.Failed)
                return notificationResult.Error;
            var notification = notificationResult.Value;

            var relatedEntityResult = GetRelatedEntity(relatedEntityUuid, relatedEntityType);
            if(relatedEntityResult.IsNone)
                return new OperationError($"Related entity of type {relatedEntityType} with uuid {relatedEntityUuid} was not found", OperationFailure.NotFound);
            var relatedEntity = relatedEntityResult.Value;

            if (_authorizationContext.AllowModify(relatedEntity))
            {
                var canBeModified = notification.IsActive;
                var canBeDeactivated = notification.IsActive && !notification.AdviceSent.Any();
                var canBeDeleted = notification.CanBeDeleted;
                if (_authorizationContext.AllowDelete(relatedEntity))
                {
                    canBeDeleted = false;
                }

                return new NotificationAccessRights(canBeDeleted, canBeDeactivated, canBeModified);
            }

            return NotificationAccessRights.ReadOnly();
        }

        private Result<(int orgId, NotificationModel model), OperationError> MapModelAndResolveOrgId(Guid organizationUuid, int relatedEntityId, ImmediateNotificationModificationParameters parameters, AdviceType adviceType)
        {
            return MapBaseModel(parameters, adviceType, relatedEntityId)
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

        private Result<NotificationModel, OperationError> MapBaseModel(ImmediateNotificationModificationParameters parameters, AdviceType adviceType, int relatedEntityId)
        {
            var model = new NotificationModel
            {
                AdviceType = adviceType,
                Body = parameters.Body,
                Subject = parameters.Subject,
                RelationId = relatedEntityId,
                Type = parameters.Type
            };
            return MapRecipients(parameters)
                .Match<Result<NotificationModel, OperationError>>(recipients =>
                {
                    var recipientList = recipients.ToList();
                    if (!recipientList.Any())
                        return new OperationError("Notification needs at least 1 recipient", OperationFailure.BadInput);

                    model.Recipients = recipientList;
                    return model;
                }, error => error);
        }

        private Result<IEnumerable<RecipientModel>, OperationError> MapRecipients(
            ImmediateNotificationModificationParameters parameters)
        {
            return MapRootRecipientModel(parameters.Ccs, RecieverType.CC, parameters.Type)
                .Bind(ccRecipients => 
                    MapRootRecipientModel(parameters.Receivers, RecieverType.RECIEVER, parameters.Type)
                        .Bind<IEnumerable<RecipientModel>>(receiverRecipients => ccRecipients.Concat(receiverRecipients).ToList())
                );
        }

        private Result<IEnumerable<RecipientModel>, OperationError> MapRootRecipientModel(
            RootRecipientModificationParameters root, RecieverType receiverType, RelatedEntityType relatedEntityType)
        {
            if (root == null)
                return new List<RecipientModel>();

            return MapRoleRecipients(root.RoleRecipients, receiverType, relatedEntityType)
                .Bind<IEnumerable<RecipientModel>>(roleRecipients => 
                    MapEmailRecipients(root.EmailRecipients, receiverType).Concat(roleRecipients).ToList());
        }

        private static IEnumerable<RecipientModel> MapEmailRecipients(IEnumerable<EmailRecipientModificationParameters> emailRecipients, RecieverType receiverType)
        {
            var recipientList = emailRecipients.ToList();
            if (!recipientList.Any())
                return new List<RecipientModel>();

            return recipientList.Select(x => new RecipientModel
            {
                Email = x.Email,
                RecipientType = RecipientType.USER,
                ReceiverType = receiverType
            });
        }

        private Result<IEnumerable<RecipientModel>, OperationError> MapRoleRecipients(IEnumerable<RoleRecipientModificationParameters> roleRecipients, RecieverType receiverType, RelatedEntityType relatedEntityType)
        {
            var recipientList = roleRecipients.ToList();
            if (!recipientList.Any())
                return new List<RecipientModel>();

            var recipients = new List<RecipientModel>();
            foreach (var recipient in recipientList)
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
            var roleIdResult = ResolveRoleId(roleUuid, relatedEntityType);
            if(roleIdResult.IsNone)
                return new OperationError($"Id for {relatedEntityType}Role with uuid: {roleUuid} was not found", OperationFailure.NotFound);
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

        private Maybe<int> ResolveRoleId(Guid roleUuid, RelatedEntityType relatedEntityType)
        {
            return relatedEntityType switch
            {
                RelatedEntityType.dataProcessingRegistration => _entityIdentityResolver.ResolveDbId<DataProcessingRegistrationRole>(roleUuid),
                RelatedEntityType.itContract => _entityIdentityResolver.ResolveDbId<ItContractRole>(roleUuid),
                RelatedEntityType.itSystemUsage => _entityIdentityResolver.ResolveDbId<ItSystemRole>(roleUuid),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null)
            };
        }

        private Result<TSuccess, OperationError> Modify<TSuccess>(Guid uuid, RelatedEntityType relatedEntityType, Func<IEntityWithAdvices, Result<TSuccess, OperationError>> mutation)
        {
            using var transaction = _transactionManager.Begin();

            var entityResult = GetRelatedEntity(uuid, relatedEntityType);

            if (entityResult.IsNone)
                return new OperationError($"Entity of type '{nameof(RelatedEntityType)}' with uuid: {uuid} was not found", OperationFailure.NotFound);
            var entity = entityResult.Value;

            if (!_authorizationContext.AllowModify(entity))
                return new OperationError(OperationFailure.Forbidden);

            var mutationResult = mutation(entity);

            if (mutationResult.Failed)
            {
                transaction.Rollback();
            }
            else
            {
                UpdateRelatedEntity(entity, relatedEntityType);
                transaction.Commit();
            }

            return mutationResult;
        }

        private Maybe<IEntityWithAdvices> GetRelatedEntity(Guid uuid, RelatedEntityType relatedEntityType)
        {
            return relatedEntityType switch
            {
                RelatedEntityType.dataProcessingRegistration => _dprRepository.AsQueryable().ByUuid(uuid),
                RelatedEntityType.itContract => _contractRepository.AsQueryable().ByUuid(uuid),
                RelatedEntityType.itSystemUsage => _usageRepository.AsQueryable().ByUuid(uuid),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null)
            };
        }

        private void UpdateRelatedEntity(IEntityWithAdvices entity, RelatedEntityType relatedEntityType)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    var dpr = entity as DataProcessingRegistration;
                    _dprRepository.Update(dpr);
                    _domainEvents.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(dpr));
                    _dprRepository.Save();
                    break;
                case RelatedEntityType.itContract:
                    var contract = entity as ItContract;
                    _contractRepository.Update(contract);
                    _domainEvents.Raise(new EntityUpdatedEvent<ItContract>(contract));
                    _contractRepository.Save();
                    break;
                case RelatedEntityType.itSystemUsage:
                    var usage = entity as ItSystemUsage;
                    _usageRepository.Update(usage);
                    _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(usage));
                    _usageRepository.Save();
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, null);
            }
        }
    }
}
