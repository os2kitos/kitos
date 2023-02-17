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
        private readonly IGenericRepository<AdviceUserRelation> _adviceUserRelationRepository;
        private readonly IDomainEvents _domainEventHandler;
        private readonly IAdviceRootResolution _adviceRootResolution;


        public RegistrationNotificationService(IAdviceService adviceService,
            IAuthorizationContext authorizationContext, 
            ITransactionManager transactionManager,
            IGenericRepository<Advice> adviceRepository,
            IDomainEvents domainEventHandler, 
            IAdviceRootResolution adviceRootResolution, IGenericRepository<AdviceUserRelation> adviceUserRelationRepository)
        {
            _adviceService = adviceService;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _adviceRepository = adviceRepository;
            _domainEventHandler = domainEventHandler;
            _adviceRootResolution = adviceRootResolution;
            _adviceUserRelationRepository = adviceUserRelationRepository;
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

        public Result<Advice, OperationError> Create(int organizationId, NotificationModel notificationModel)
        {
            var newNotification = MapCreateModelToEntity(notificationModel);

            if (!_authorizationContext.AllowModify(ResolveRoot(newNotification)))
            {
                return new OperationError($"User is not allowed to create notification in organization with id: {organizationId}, for root type with id: {newNotification.RelationId}", OperationFailure.Forbidden);
            }

            //Prepare new advice
            newNotification.IsActive = true;
            if (newNotification.AdviceType == AdviceType.Immediate)
            {
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

        public Result<Advice, OperationError> Update(int notificationId, UpdateNotificationModel notificationModel)
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

            BaseMapModelToEntity(notificationModel, entity);
            
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

        public Maybe<OperationError> DeleteUserRelationByAdviceId(int notificationId)
        {
            foreach (var d in _adviceUserRelationRepository.AsQueryable().Where(d => d.AdviceId == notificationId).ToList())
            {
                if (_authorizationContext.AllowDelete(d))
                {
                    _adviceUserRelationRepository.Delete(d);
                    _adviceUserRelationRepository.Save();
                }
                else
                {
                    return new OperationError($"User is not allowed to delete user relation with adviceId: {notificationId}", OperationFailure.Forbidden);
                }
            }
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
        
        private static Advice MapCreateModelToEntity(NotificationModel model)
        {
            var notification = new Advice();
            BaseMapModelToEntity(model, notification);
            notification.Scheduling = model.RepetitionFrequency;
            notification.AlarmDate = model.FromDate;
            notification.Reciepients = model.Recipients.Select(MapToAdviceUserRelation).ToList();

            return notification;
        }

        private static void BaseMapModelToEntity<T>(T model, Advice notification) where T: UpdateNotificationModel
        {
            notification.Name = model.Name;
            notification.StopDate = model.ToDate;
            notification.Subject = model.Subject;
            notification.Body = model.Body;
            notification.RelationId = model.RelationId;
            notification.Type = model.Type;
            notification.AdviceType = model.AdviceType;
        }

        private static AdviceUserRelation MapToAdviceUserRelation<T>(T model) where T: RecipientModel
        {
            return new AdviceUserRelation
            {
                Email = model.Email,
                DataProcessingRegistrationRoleId = model.DataProcessingRegistrationRoleId,
                ItContractRoleId = model.ItContractRoleId,
                ItSystemRoleId = model.ItSystemRoleId,
                RecieverType = model.ReceiverType,
                RecpientType = model.RecipientType
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
