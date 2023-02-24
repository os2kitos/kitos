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
using Core.DomainModel.Notification;
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
                    return _registrationNotificationService.GetNotificationsByOrganizationId(orgId)
                        .Select(baseQuery => ApplyQuery(baseQuery, conditions));
                });
        }

        public Result<Advice, OperationError> GetNotificationByUuid(Guid uuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return _entityIdentityResolver.ResolveDbId<Advice>(uuid)
                .Match(_registrationNotificationService.GetNotificationById, 
                    () => new OperationError($"Id for notification with uuid: {uuid} was not found", OperationFailure.NotFound))
                .Bind(notification => VerifyCanNotificationBeReadAndReturnNotification(notification, relatedEntityUuid, relatedEntityType));
        }

        public Result<IEnumerable<AdviceSent>, OperationError> GetNotificationSentByUuid(Guid uuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return GetNotificationByUuid(uuid, relatedEntityUuid, relatedEntityType)
                .Bind(notification => 
                    Result<IEnumerable<AdviceSent>, OperationError>.Success(GetSentFilteredByNotificationUuidAndType(uuid, notification.RelationId, relatedEntityType))
                );
        }

        public Result<Advice, OperationError> CreateImmediateNotification(ImmediateNotificationModificationParameters parameters)
        {
            return Modify(parameters.BaseProperties.OwnerResourceUuid, parameters.BaseProperties.Type, relatedEntity =>
            {
               return MapImmediateModel(parameters, AdviceType.Immediate, relatedEntity.Id)
                    .Bind(result => _registrationNotificationService.CreateImmediateNotification(result));
            });
        }

        public Result<Advice, OperationError> CreateScheduledNotification(CreateScheduledNotificationModificationParameters parameters)
        {
            return Modify(parameters.BaseProperties.OwnerResourceUuid, parameters.BaseProperties.Type, relatedEntity =>
            {
                return MapCreateScheduledModel(parameters, relatedEntity.Id)
                    .Bind(result => _registrationNotificationService.CreateScheduledNotification(result));
            });
        }

        public Result<Advice, OperationError> UpdateScheduledNotification(Guid notificationUuid, UpdateScheduledNotificationModificationParameters parameters)
        {
            return Modify(parameters.BaseProperties.OwnerResourceUuid, parameters.BaseProperties.Type, relatedEntity =>
            {
                var model = MapUpdateScheduledModel(parameters, relatedEntity.Id);
                return MapRecipients(parameters)
                    .Bind(recipientsResult =>
                    {
                        return ResolveNotificationIdAndUpdateNotificationRelations(notificationUuid, model.BaseProperties.Type, recipientsResult.ccs, recipientsResult.receivers)
                            .Bind(notificationId => _registrationNotificationService.Update(notificationId, model));
                    });
            });
        }

        public Result<Advice, OperationError> DeactivateNotification(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return GetPermissionsWithNotification(notificationUuid, relatedEntityUuid, relatedEntityType)
                .Bind(result => result.permissions.Deactivate 
                        ? _registrationNotificationService.DeactivateNotification(result.notification.Id)
                        : new OperationError($"User is not allowed to deactivate notification with uuid: {notificationUuid}", OperationFailure.Forbidden));
        }

        public Maybe<OperationError> DeleteNotification(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return GetPermissionsWithNotification(notificationUuid, relatedEntityUuid, relatedEntityType)
                .Match(result => result.permissions.Delete
                    ? _registrationNotificationService.Delete(result.notification.Id)
                    : new OperationError($"User is not allowed to delete notification with uuid: {notificationUuid}", OperationFailure.Forbidden),
                    error => error);
        }
        
        public Result<NotificationPermissions, OperationError> GetPermissions(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return _entityIdentityResolver.ResolveDbId<Advice>(notificationUuid)
                .Match
                (
                    _registrationNotificationService.GetNotificationById,
                    () => new OperationError($"Id for notification with uuid: {notificationUuid} was not found", OperationFailure.NotFound)
                )
                .Bind(notification =>
                {
                    return GetRelatedEntity(relatedEntityUuid, relatedEntityType)
                        .Match(relatedEntity => NotificationPermissions.FromResolutionResult(notification, relatedEntity, _authorizationContext),
                            () => new OperationError($"Related entity of type {relatedEntityType} with uuid {relatedEntityUuid} was not found", OperationFailure.NotFound));
                });
        }

        private Result<int, OperationError> ResolveOrganizationId(Guid organizationUuid)
        {
            return _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid)
                .Match<Result<int, OperationError>>
                (
                    id => id,
                    () => new OperationError($"Id for Organization with uuid: {organizationUuid} was not found", OperationFailure.NotFound)
                );
        }

        private Result<Advice, OperationError> VerifyCanNotificationBeReadAndReturnNotification(Advice notification, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return GetRelatedEntity(relatedEntityUuid, relatedEntityType)
                .Match<Result<Advice, OperationError>>(relatedEntity =>
                {
                    if (!_authorizationContext.AllowReads(relatedEntity))
                        return new OperationError($"User not allowed to read the notification with uuid: {notification.Uuid}", OperationFailure.Forbidden);

                    if (notification.Type != relatedEntityType)
                        return new OperationError($"Notification related entity type is different than {relatedEntityType}", OperationFailure.BadInput);

                    return notification;
                }, () => new OperationError("Related entity was not found", OperationFailure.NotFound));
        }

        private Result<(Advice notification, NotificationPermissions permissions), OperationError> GetPermissionsWithNotification(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return GetNotificationByUuid(notificationUuid, relatedEntityUuid, relatedEntityType)
                .Bind(notification => GetPermissions(notificationUuid, relatedEntityUuid, relatedEntityType)
                    .Select(permissions => (notification, permissions))
                );
        }

        private Result<int, OperationError> ResolveNotificationIdAndUpdateNotificationRelations(Guid notificationUuid,
            RelatedEntityType relatedEntityType, RecipientModel ccs, RecipientModel receivers)
        {
            return _entityIdentityResolver.ResolveDbId<Advice>(notificationUuid)
                .Match(notificationId =>
                    {
                        return _notificationUserRelationsService
                            .UpdateNotificationUserRelations(notificationId, ccs, receivers, relatedEntityType)
                            .Match<Result<int, OperationError>>(error => error, () => notificationId);
                    },
                    () => new OperationError($"Id for notification with uuid: {notificationUuid} was not found",
                        OperationFailure.NotFound));
        }

        private Result<ImmediateNotificationModel, OperationError> MapImmediateModel(ImmediateNotificationModificationParameters parameters, AdviceType adviceType, int relatedEntityId)
        {
            var model = MapBaseProperties<ImmediateNotificationModificationParameters, ImmediateNotificationModel>(parameters, adviceType, relatedEntityId);

            return MapRecipientsToModel(parameters, model);
        }

        private Result<ScheduledNotificationModel, OperationError> MapCreateScheduledModel(CreateScheduledNotificationModificationParameters parameters, int relatedEntityId)
        {
            var model = MapScheduledBaseProperties<CreateScheduledNotificationModificationParameters, ScheduledNotificationModel>(parameters, relatedEntityId);
            model.FromDate = parameters.FromDate;
            model.RepetitionFrequency = parameters.RepetitionFrequency;
            return MapRecipientsToModel(parameters, model);
        }

        private UpdateScheduledNotificationModel MapUpdateScheduledModel(UpdateScheduledNotificationModificationParameters parameters, int relatedEntityId)
        {
            return MapScheduledBaseProperties<UpdateScheduledNotificationModificationParameters, UpdateScheduledNotificationModel>(parameters, relatedEntityId);
        }

        private Result<TResult, OperationError> MapRecipientsToModel<TParameters, TResult>(TParameters parameters, TResult model) 
            where TParameters: class, IHasBaseNotificationPropertiesParameters
            where TResult: class, IHasBaseNotificationPropertiesModel, IHasRecipientModels, new()
        {
            return MapRecipients(parameters)
                .Select(result =>
                {
                    model.Ccs = result.ccs;
                    model.Receivers = result.receivers;

                    return model;
                });
        }

        private Result<(RecipientModel ccs, RecipientModel receivers), OperationError> MapRecipients<TParameters>(TParameters parameters) 
            where TParameters: class, IHasBaseNotificationPropertiesParameters
        {
            return MapRootRecipientModel(parameters.BaseProperties.Receivers, parameters.BaseProperties.Type)
                .Bind(receivers =>
                {
                    if (!receivers.RoleRecipients.Any() && !receivers.EmailRecipients.Any())
                        return new OperationError("At least 1 receiver is required", OperationFailure.BadInput);

                    return MapRootRecipientModel(parameters.BaseProperties.Ccs, parameters.BaseProperties.Type)
                        .Select(ccs => (ccs, receivers));
                });
        }

        private static TResult MapScheduledBaseProperties<TParameters, TResult>(TParameters parameters, int relatedEntityId)
            where TParameters : class, IHasBaseNotificationPropertiesParameters, IHasReadonlyName, IHasReadonlyToDate
            where TResult : class, IHasBaseNotificationPropertiesModel, IHasName, IHasToDate, new()
        {
            var model = MapBaseProperties<TParameters, TResult>(parameters, AdviceType.Repeat, relatedEntityId);
            model.Name = parameters.Name;
            model.ToDate = parameters.ToDate;

            return model;
        }

        private static TResult MapBaseProperties<TParameters, TResult>(TParameters parameters, AdviceType adviceType,
            int relatedEntityId)
            where TParameters : class, IHasBaseNotificationPropertiesParameters
            where TResult : class, IHasBaseNotificationPropertiesModel, new()
        {
            return new TResult
            {
                BaseProperties = new BaseNotificationPropertiesModel
                {
                    AdviceType = adviceType,
                    Body = parameters.BaseProperties.Body,
                    Subject = parameters.BaseProperties.Subject,
                    RelationId = relatedEntityId,
                    Type = parameters.BaseProperties.Type
                }
            };
        }

        private Result<RecipientModel, OperationError> MapRootRecipientModel(
            RootRecipientModificationParameters root, RelatedEntityType relatedEntityType)
        {
            if (root == null)
                return null;

            return MapRoleRecipients(root.RoleRecipients, relatedEntityType)
                .Bind<RecipientModel>(roleRecipients => new RecipientModel()
                {
                    RoleRecipients = roleRecipients,
                    EmailRecipients = MapEmailRecipients(root.EmailRecipients)
                });
        }

        private static IEnumerable<EmailRecipientModel> MapEmailRecipients(IEnumerable<EmailRecipientModificationParameters> emailRecipients)
        {
            var recipientList = emailRecipients.ToList();
            if (!recipientList.Any())
                return new List<EmailRecipientModel>();

            return recipientList.Select(x => new EmailRecipientModel
            {
                Email = x.Email
            });
        }

        private Result<IEnumerable<RoleRecipientModel>, OperationError> MapRoleRecipients(IEnumerable<RoleRecipientModificationParameters> roleRecipients, RelatedEntityType relatedEntityType)
        {
            var recipientList = roleRecipients.ToList();
            if (!recipientList.Any())
                return new List<RoleRecipientModel>();

            var recipients = new List<RoleRecipientModel>();
            foreach (var recipient in recipientList)
            {
                var idResult = ResolveRoleId(recipient.RoleUuid, relatedEntityType);
                if (idResult.IsNone)
                    return new OperationError($"Id for {relatedEntityType}Role with uuid: {recipient.RoleUuid} was not found", OperationFailure.NotFound); ;

                recipients.Add(new RoleRecipientModel{RoleId = idResult.Value});
            }

            return recipients;
        }

        private IEnumerable<AdviceSent> GetSentFilteredByNotificationUuidAndType(Guid uuid, int? relationId, RelatedEntityType relatedEntityType)
        {
            return _registrationNotificationService.GetSent()
                .Where(x => x.Advice.Uuid == uuid && x.Advice.RelationId == relationId && x.Advice.Type == relatedEntityType).ToList();
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

        private static IQueryable<Advice> ApplyQuery(IQueryable<Advice> baseQuery, params IDomainQuery<Advice>[] conditions)
        {
            var subQueries = new List<IDomainQuery<Advice>>();
            subQueries.AddRange(conditions);

            return subQueries.Any()
                ? new IntersectionQuery<Advice>(subQueries).Apply(baseQuery)
                : baseQuery;
        }

        private Result<TSuccess, OperationError> Modify<TSuccess>(Guid relatedEntityUuid, RelatedEntityType relatedEntityType, Func<IEntityWithAdvices, Result<TSuccess, OperationError>> mutation)
        {
            using var transaction = _transactionManager.Begin();

            var entityResult = GetRelatedEntity(relatedEntityUuid, relatedEntityType);

            if (entityResult.IsNone)
                return new OperationError($"Entity of type '{nameof(RelatedEntityType)}' with uuid: {relatedEntityUuid} was not found", OperationFailure.NotFound);
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
