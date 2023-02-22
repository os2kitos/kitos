using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Shared;
using Core.DomainServices;
using Core.DomainServices.Advice;
using Core.DomainServices.Authorization;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Notification
{
    public class RegistrationNotificationService : IRegistrationNotificationService
    {
        private readonly IAdviceService _adviceService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IDomainEvents _domainEventHandler;
        private readonly IAdviceRootResolution _adviceRootResolution;


        public RegistrationNotificationService(IAdviceService adviceService,
            IAuthorizationContext authorizationContext, 
            ITransactionManager transactionManager,
            IGenericRepository<Advice> adviceRepository,
            IDomainEvents domainEventHandler, 
            IAdviceRootResolution adviceRootResolution)
        {
            _adviceService = adviceService;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _adviceRepository = adviceRepository;
            _domainEventHandler = domainEventHandler;
            _adviceRootResolution = adviceRootResolution;
        }

        public IQueryable<Advice> GetCurrentUserNotifications()
        {
            return _adviceService.GetAdvicesAccessibleToCurrentUser();
        }

        public Result<IQueryable<Advice>, OperationError> GetNotificationsByOrganizationId(int organizationId)
        {
            return _authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All 
                ? new OperationError($"User is not allowed to read organization with id: {organizationId}", OperationFailure.Forbidden) 
                : Result<IQueryable<Advice>, OperationError>.Success(_adviceService.GetAdvicesForOrg(organizationId));
        }

        public Maybe<Advice> GetNotificationById(int id)
        {
            return _adviceService.GetAdviceById(id);
        }

        public IQueryable<AdviceSent> GetSent()
        {
            return _adviceService
                .GetAdvicesAccessibleToCurrentUser()
                .SelectMany(x => x.AdviceSent);
        }

        public Result<Advice, OperationError> Create(NotificationModel notificationModel)
        {
            var newNotification = MapCreateModelToEntity(notificationModel);

            if (!_authorizationContext.AllowModify(ResolveRoot(newNotification)))
            {
                return new OperationError($"User is not allowed to create notification for root type with id: {newNotification.RelationId}", OperationFailure.Forbidden);
            }

            //Prepare new advice
            newNotification.IsActive = true;
            if (newNotification.AdviceType == AdviceType.Immediate)
            {
                newNotification.Scheduling = Scheduling.Immediate;
                newNotification.StopDate = null;
                newNotification.AlarmDate = null;
            }

            using var transaction = _transactionManager.Begin();

            _adviceRepository.Insert(newNotification);
            _domainEventHandler.Raise(new EntityCreatedEvent<Advice>(newNotification));
            _adviceRepository.Save();

            var name = Advice.CreateJobId(newNotification.Id);

            newNotification.JobId = name;
            newNotification.IsActive = true;
            
            //Sends a notification and updates the job id
            _adviceService.CreateAdvice(newNotification);

            transaction.Commit();
            return newNotification;
        }

        public Result<Advice, OperationError> Update(int notificationId, BaseNotificationModel notificationModel)
        {
            using var transaction = _transactionManager.Begin();

            var entityResult = GetNotificationById(notificationId);
            if (entityResult.IsNone)
                return new OperationError($"Notification with Id: {notificationId} was not found", OperationFailure.NotFound);
            var entity = entityResult.Value;
            if (!_authorizationContext.AllowModify(entity))
            {
                return new OperationError($"User is not allowed to modify notification with id: {notificationId}", OperationFailure.Forbidden);
            }

            MapBasePropertiesToNotification(notificationModel, entity);
            
            _adviceRepository.Update(entity);
            RaiseAsRootModification(entity);
            _adviceRepository.Save();

            _adviceService.UpdateSchedule(entity);

            transaction.Commit();

            return entity;
        }

        public Maybe<OperationError> Delete(int notificationId)
        {
            using var transaction = _transactionManager.Begin();
            var entityResult = _adviceService.GetAdviceById(notificationId);
            if (entityResult.IsNone)
            {
                return new OperationError($"Notification with id {notificationId} was not found", OperationFailure.NotFound);
            }

            var entity = entityResult.Value;

            if (!_authorizationContext.AllowDelete(entity))
            {
                return new OperationError($"User is not allowed to delete notification with id: {notificationId}", OperationFailure.Forbidden);
            }

            if (!entity.CanBeDeleted)
            {
                return new OperationError("Cannot delete advice which is active or has been sent", OperationFailure.BadState);
            }

            RaiseAsRootModification(entity);
            _adviceService.Delete(entity);
            
            transaction.Commit();

            return Maybe<OperationError>.None;
        }

        public Result<Advice, OperationError> DeactivateNotification(int adviceId)
        {
            using var transaction = _transactionManager.Begin();
            var entityResult = _adviceService.GetAdviceById(adviceId);
            if (entityResult.IsNone)
            {
                return new OperationError($"Notification with id {adviceId} was not found", OperationFailure.NotFound);
            }
            var entity = entityResult.Value;

            if (!_authorizationContext.AllowModify(entity))
            {
                return new OperationError($"User is not allowed to deactivate notification with id: {adviceId}", OperationFailure.Forbidden);
            }
            _adviceService.Deactivate(entity);
            RaiseAsRootModification(entity);
            transaction.Commit();

            return entity;
        }

        private static Advice MapCreateSchedulingModelToEntity(ScheduledNotificationModel model)
        {
            notification.Name = model.Name;
            notification.StopDate = model.ToDate;
            return new Advice();
        }

        private static Advice MapCreateModelToEntity<T>(T model) where T : class, IHasBasePropertiesModel, IHasRecipientModels
        {
            var notification = new Advice();
            MapBasePropertiesToNotification(model, notification);
            notification.Scheduling = model.RepetitionFrequency;
            notification.AlarmDate = model.FromDate;
            var recipients = 
            notification.Reciepients = model.Recipients.Select(MapToAdviceUserRelation).ToList();

            return notification;
        }

        private static void MapBasePropertiesToNotification<T>(T model, Advice notification) where T: class, IHasBasePropertiesModel, IHasRecipientModels
        {
            notification.Subject = model.BaseProperties.Subject;
            notification.Body = model.BaseProperties.Body;
            notification.RelationId = model.BaseProperties.RelationId;
            notification.Type = model.BaseProperties.Type;
            notification.AdviceType = model.BaseProperties.AdviceType;
        }

        private static IEnumerable<AdviceUserRelation> MapToAdviceUserRelation(RecipientModel model, RecieverType receiverType, RelatedEntityType relatedEntityType)
        {
            var recipients = new List<AdviceUserRelation>();
            
            recipients.AddRange(model.EmailRecipients.Select(x => MapEmailRecipientToRelation(x, receiverType)).ToList());
            recipients.AddRange(model.RoleRecipients.Select(x => MapRoleRecipientToRelation(x, receiverType, relatedEntityType)).ToList());

            return recipients;
        }

        private static AdviceUserRelation MapEmailRecipientToRelation(EmailRecipientModel model, RecieverType receiverType)
        {
            return new AdviceUserRelation
            {
                Email = model.Email,
                RecpientType = RecipientType.USER,
                RecieverType = receiverType
            };
        }

        private static AdviceUserRelation MapRoleRecipientToRelation(RoleRecipientModel model, RecieverType receiverType, RelatedEntityType relatedEntityType)
        {
            return new AdviceUserRelation
            {
                ItContractRoleId = relatedEntityType == RelatedEntityType.itContract ? model.RoleId : null,
                ItSystemRoleId = relatedEntityType == RelatedEntityType.itSystemUsage ? model.RoleId : null,
                DataProcessingRegistrationRoleId = relatedEntityType == RelatedEntityType.dataProcessingRegistration ? model.RoleId : null,
                RecpientType = RecipientType.USER,
                RecieverType = receiverType
            };
        }

        private IEntityWithAdvices ResolveRoot(Advice advice)
        {
            return _adviceRootResolution.Resolve(advice).GetValueOrDefault();
        }
 
        private void RaiseAsRootModification(Advice entity)
        {
            switch (ResolveRoot(entity))
            {
                case ItContract root:
                    _domainEventHandler.Raise(new EntityUpdatedEvent<ItContract>(root));
                    break;
                case ItSystemUsage root:
                    _domainEventHandler.Raise(new EntityUpdatedEvent<ItSystemUsage>(root));
                    break;
                case DataProcessingRegistration root:
                    _domainEventHandler.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(root));
                    break;
            }
        }
    }
}
