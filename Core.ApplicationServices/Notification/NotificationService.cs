using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Read;
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

        public Result<IEnumerable<NotificationResultModel>, OperationError> GetNotifications(Guid organizationUuid, int? page, int? pageSize, params IDomainQuery<Advice>[] conditions)
        {
            return ResolveOrganizationId(organizationUuid)
                .Bind(_registrationNotificationService.GetNotificationsByOrganizationId)
                .Select(baseQuery => ApplyQuery(baseQuery, page, pageSize, conditions).ToList())
                .Bind(MapNotificationsToResultModelList);
        }

        public Result<NotificationResultModel, OperationError> GetNotificationByUuid(Guid uuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return ResolveNotificationId(uuid)
                .Bind(_registrationNotificationService.GetNotificationById)
                .Bind(notification => WithReadAccess(notification, relatedEntityUuid, relatedEntityType))
                .Bind(MapNotificationToResultModel);
        }

        public Result<IEnumerable<AdviceSent>, OperationError> GetNotificationSentByUuid(Guid uuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return GetNotificationByUuid(uuid, relatedEntityUuid, relatedEntityType)
                .Select(notification => GetSentFilteredByNotificationUuidAndType(uuid, notification.OwnerResource.Id, relatedEntityType));
        }

        public Result<NotificationResultModel, OperationError> CreateImmediateNotification(ImmediateNotificationModificationParameters parameters)
        {
            return Modify(parameters.BaseProperties.OwnerResourceUuid, parameters.BaseProperties.Type, relatedEntity =>
            {
               return MapImmediateModel(parameters, relatedEntity.Id)
                    .Bind(result => _registrationNotificationService.CreateImmediateNotification(result))
                    .Bind(MapNotificationToResultModel);
            });
        }

        public Result<NotificationResultModel, OperationError> CreateScheduledNotification(CreateScheduledNotificationModificationParameters parameters)
        {
            return Modify(parameters.BaseProperties.OwnerResourceUuid, parameters.BaseProperties.Type, relatedEntity =>
            {
                return MapCreateScheduledModel(parameters, relatedEntity.Id)
                    .Bind(result => _registrationNotificationService.CreateScheduledNotification(result))
                    .Bind(MapNotificationToResultModel);
            });
        }

        public Result<NotificationResultModel, OperationError> UpdateScheduledNotification(Guid notificationUuid, UpdateScheduledNotificationModificationParameters parameters)
        {
            return Modify(parameters.BaseProperties.OwnerResourceUuid, parameters.BaseProperties.Type, relatedEntity =>
            {
                var model = MapUpdateScheduledModel(parameters, relatedEntity.Id);
                return MapRecipients(parameters)
                    .Bind(recipientsResult =>
                    {
                        return ResolveNotificationId(notificationUuid)
                            .Bind(notificationId => _notificationUserRelationsService.UpdateNotificationUserRelations(notificationId, recipientsResult.ccs, recipientsResult.receivers, parameters.BaseProperties.Type))
                            .Bind(notification => _registrationNotificationService.Update(notification.Id, model));
                    })
                    .Bind(MapNotificationToResultModel);
            });
        }

        public Result<NotificationResultModel, OperationError> DeactivateNotification(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return GetPermissionsWithNotification(notificationUuid, relatedEntityUuid, relatedEntityType)
                .Bind(result => result.permissions.Deactivate 
                        ? _registrationNotificationService.DeactivateNotification(result.notification.Id)
                        : new OperationError($"User is not allowed to deactivate notification with uuid: {notificationUuid}", OperationFailure.Forbidden))
                .Bind(MapNotificationToResultModel);
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
            return ResolveNotificationId(notificationUuid)
                .Bind(_registrationNotificationService.GetNotificationById)
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

        private Result<Advice, OperationError> WithReadAccess(Advice notification, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
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

        private Result<(NotificationResultModel notification, NotificationPermissions permissions), OperationError> GetPermissionsWithNotification(Guid notificationUuid, Guid relatedEntityUuid, RelatedEntityType relatedEntityType)
        {
            return GetNotificationByUuid(notificationUuid, relatedEntityUuid, relatedEntityType)
                .Bind(notification => GetPermissions(notificationUuid, relatedEntityUuid, relatedEntityType)
                    .Select(permissions => (notification, permissions))
                );
        }

        private Result<int, OperationError> ResolveNotificationId(Guid notificationUuid)
        {
            return _entityIdentityResolver.ResolveDbId<Advice>(notificationUuid)
                .Match<Result<int, OperationError>>(
                    notificationId => notificationId,
                    () => new OperationError($"Id for notification with uuid: {notificationUuid} was not found",
                        OperationFailure.NotFound));
        }

        private Result<ImmediateNotificationModel, OperationError> MapImmediateModel(ImmediateNotificationModificationParameters parameters, int relatedEntityId)
        {
            return MapRecipientsToModel(parameters)
                .Select(result =>
                    new ImmediateNotificationModel
                    (
                        MapBaseProperties(parameters, AdviceType.Immediate, relatedEntityId),
                        result.ccs,
                        result.receivers
                    )
                );
        }

        private Result<ScheduledNotificationModel, OperationError> MapCreateScheduledModel(CreateScheduledNotificationModificationParameters parameters, int relatedEntityId)
        {
            return MapRecipientsToModel(parameters)
                .Select(result =>
                    new ScheduledNotificationModel
                        (
                            parameters.Name,
                            parameters.ToDate,
                            parameters.RepetitionFrequency,
                            parameters.FromDate,
                            MapBaseProperties(parameters, AdviceType.Repeat, relatedEntityId),
                            result.ccs,
                            result.receivers
                        )
                );
        }

        private static UpdateScheduledNotificationModel MapUpdateScheduledModel(UpdateScheduledNotificationModificationParameters parameters, int relatedEntityId)
        {
            return new UpdateScheduledNotificationModel
            (
                parameters.Name,
                parameters.ToDate,
                MapBaseProperties(parameters, AdviceType.Repeat, relatedEntityId)
            );
        }

        private Result<(RecipientModel ccs, RecipientModel receivers), OperationError> MapRecipientsToModel<TParameters>(TParameters parameters) 
            where TParameters: class, IHasBaseNotificationPropertiesParameters
        {
            return MapRecipients(parameters)
                .Select(result => (result.ccs, result.receivers));
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

        private static BaseNotificationPropertiesModel MapBaseProperties<TParameters>(TParameters parameters, AdviceType adviceType, int relatedEntityId)
            where TParameters : class, IHasBaseNotificationPropertiesParameters
        {
            return new BaseNotificationPropertiesModel(
                    parameters.BaseProperties.Subject,
                    parameters.BaseProperties.Body,
                    parameters.BaseProperties.Type,
                    adviceType,
                    relatedEntityId
                    );
        }

        private Result<RecipientModel, OperationError> MapRootRecipientModel(
            RootRecipientModificationParameters root, RelatedEntityType relatedEntityType)
        {
            return MapRoleRecipients(root.RoleRecipients, relatedEntityType)
                .Bind<RecipientModel>(roleRecipients => new RecipientModel
                    (
                        MapEmailRecipients(root.EmailRecipients), 
                        roleRecipients
                    ));
        }

        private static IEnumerable<EmailRecipientModel> MapEmailRecipients(IEnumerable<EmailRecipientModificationParameters> emailRecipients)
        {
            var recipientList = emailRecipients.ToList();
            return !recipientList.Any() 
                ? new List<EmailRecipientModel>() 
                : recipientList.Select(x => new EmailRecipientModel(x.Email)).ToList();
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
                    return new OperationError($"Id for {relatedEntityType}Role with uuid: {recipient.RoleUuid} was not found", OperationFailure.NotFound);

                recipients.Add(new RoleRecipientModel(idResult.Value));
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

        private static IQueryable<Advice> ApplyQuery(IQueryable<Advice> baseQuery, int? page, int? pageSize, params IDomainQuery<Advice>[] conditions)
        {
            var subQueries = new List<IDomainQuery<Advice>>();
            subQueries.AddRange(conditions);

            var query = subQueries.Any()
                ? new IntersectionQuery<Advice>(subQueries).Apply(baseQuery)
                : baseQuery;

            var offsetResult = query.OrderBy(x => x.Id)
                .Skip(page.GetValueOrDefault(0) * pageSize.GetValueOrDefault(0));

            return pageSize.HasValue
                ? offsetResult.Take(pageSize.Value)
                : offsetResult;
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
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, $"NotificationService:GetRelatedEntity doesn't support value: {nameof(RelatedEntityType)}")
            };
        }

        private Maybe<IEntityWithAdvices> GetRelatedEntity(int id, RelatedEntityType relatedEntityType)
        {
            return relatedEntityType switch
            {
                RelatedEntityType.dataProcessingRegistration => _dprRepository.AsQueryable().ById(id),
                RelatedEntityType.itContract => _contractRepository.AsQueryable().ById(id),
                RelatedEntityType.itSystemUsage => _usageRepository.AsQueryable().ById(id),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, $"NotificationService:GetRelatedEntity doesn't support value: {nameof(RelatedEntityType)}")
            };
        }

        private void UpdateRelatedEntity(IEntityWithAdvices entity, RelatedEntityType relatedEntityType)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.dataProcessingRegistration:
                    var dpr = (DataProcessingRegistration)entity;
                    dpr.MarkAsDirty();
                    _domainEvents.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(dpr));
                    _dprRepository.Save();
                    break;
                case RelatedEntityType.itContract:
                    var contract = (ItContract)entity;
                    contract.MarkAsDirty();
                    _domainEvents.Raise(new EntityUpdatedEvent<ItContract>(contract));
                    _contractRepository.Save();
                    break;
                case RelatedEntityType.itSystemUsage:
                    var usage = (ItSystemUsage)entity;
                    usage.MarkAsDirty();
                    _domainEvents.Raise(new EntityUpdatedEvent<ItSystemUsage>(usage));
                    _usageRepository.Save();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(relatedEntityType), relatedEntityType, $"NotificationService:UpdateRelatedEntity doesn't support value: {nameof(RelatedEntityType)}");
            }
        }

        private Result<IEnumerable<NotificationResultModel>, OperationError> MapNotificationsToResultModelList(IEnumerable<Advice> notifications)
        {
            var result = new List<NotificationResultModel>();
            foreach (var notification in notifications.ToList())
            {
                var modelResult = MapNotificationToResultModel(notification);
                if (modelResult.Failed)
                    return modelResult.Error;

                result.Add(modelResult.Value);
            }

            return result;
        }

        private Result<NotificationResultModel, OperationError> MapNotificationToResultModel(Advice notification)
        {
            if (!notification.RelationId.HasValue)
                return new OperationError($"Notification with uuid: {notification.Uuid} has no RelationId", OperationFailure.BadState);
            
            return GetRelatedEntity(notification.RelationId.Value, notification.Type)
                .Match(ownerResource => MapRecipientsToResult(notification)
                    .Select(recipientResult => 
                        new NotificationResultModel
                        (
                            notification.Id,
                            notification.Uuid,
                            notification.IsActive,
                            notification.Name,
                            notification.AlarmDate,
                            notification.StopDate,
                            notification.SentDate,
                            notification.Subject,
                            notification.Body,
                            notification.Scheduling,
                            ownerResource,
                            notification.Type,
                            notification.AdviceType,
                            recipientResult.ccs,
                            recipientResult.receivers
                        )
                    ),
                    () => new OperationError($"Owner resource for notification with uuid: {notification.Uuid} was not found", OperationFailure.NotFound)
                );
        }
        
        private Result<(RecipientResultModel ccs, RecipientResultModel receivers), OperationError> MapRecipientsToResult(Advice notification)
        {
            return MapRecipientsToResultByType(notification.Reciepients, RecieverType.CC)
                .Bind(ccs => MapRecipientsToResultByType(notification.Reciepients, RecieverType.RECIEVER)
                    .Select(receivers => (ccs, receivers))
                );
        }

        private Result<RecipientResultModel, OperationError> MapRecipientsToResultByType(IEnumerable<AdviceUserRelation> recipients, RecieverType receiverType)
        {
            var recipientList = recipients.ToList();
            return MapRoleRecipientToResponse(recipientList, receiverType)
                .Select(roles => new RecipientResultModel
                    (
                        MapEmailRecipientToResponse(recipientList, receiverType),
                        roles
                    )
                );
        }

        private static IEnumerable<EmailRecipientResultModel> MapEmailRecipientToResponse(IEnumerable<AdviceUserRelation> recipients, RecieverType receiverType)
        {
            return recipients
                .Where(x => x.RecieverType == receiverType && x.RecpientType == RecipientType.USER)
                .Select(x => new EmailRecipientResultModel(x.Email))
                .ToList();
        }

        private static Result<IEnumerable<RoleRecipientResultModel>, OperationError> MapRoleRecipientToResponse(IEnumerable<AdviceUserRelation> recipients, RecieverType receiverType)
        {
            var result = new List<RoleRecipientResultModel>();
            var roleRecipients = recipients
                .Where(x => x.RecieverType == receiverType && x.RecpientType == RecipientType.ROLE)
                .ToList();

            foreach (var roleRecipient in roleRecipients)
            {
                var role = roleRecipient.DataProcessingRegistrationRole ?? roleRecipient.ItContractRole ?? (IRoleEntity) roleRecipient.ItSystemRole;
                if(role == null) 
                    return new OperationError($"Role wasn't found for AdviceUserRelation with id: {roleRecipient.Id}", OperationFailure.NotFound);

                result.Add(new RoleRecipientResultModel(role));
            }

            return result;
        }
    }
}
