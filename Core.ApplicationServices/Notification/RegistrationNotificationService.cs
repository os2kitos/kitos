using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification.Write;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Advice;
using Core.DomainServices.Authorization;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Notification
{
    public class RegistrationNotificationService : IRegistrationNotificationService
    {
        private readonly IAdviceService _adviceService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IOperationClock _operationClock;
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IGenericRepository<AdviceUserRelation> _adviceUserRelationRepository;
        private readonly IDomainEvents _domainEventHandler;
        private readonly IAdviceRootResolution _adviceRootResolution;


        public RegistrationNotificationService(IAdviceService adviceService,
            IAuthorizationContext authorizationContext, 
            ITransactionManager transactionManager,
            IOperationClock operationClock,
            IGenericRepository<Advice> adviceRepository,
            IDomainEvents domainEventHandler, 
            IAdviceRootResolution adviceRootResolution, IGenericRepository<AdviceUserRelation> adviceUserRelationRepository)
        {
            _adviceService = adviceService;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _operationClock = operationClock;
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
            if(_authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
                return new OperationError($"User is not allowed to read organization with id: {organizationId}", OperationFailure.Forbidden);

            return Result<IQueryable<Advice>, OperationError>.Success(_adviceService.GetAdvicesForOrg(organizationId));
        }

        public Result<Advice, OperationError> GetNotificationById(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<AdviceSent> GetSent()
        {
            return _adviceService
                .GetAdvicesAccessibleToCurrentUser()
                .SelectMany(x => x.AdviceSent);
        }

        public Result<Advice, OperationError> Create(int organizationId, NotificationModificationModel notificationModel)
        {
            if (!_authorizationContext.AllowCreate<Advice>(organizationId))
            {
                return new OperationError($"User is not allowed to create notification in organization with id: {organizationId}", OperationFailure.Forbidden);
            }

            var newNotification = MapModelToAdvice(notificationModel);

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

        public Result<Advice, OperationError> Update(int organizationId, UpdateNotificationModificationModel notificationModel)
        {
            throw new NotImplementedException();
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

        private Advice MapModelToAdvice(NotificationModificationModel model)
        {
            return new Advice
            {
                Name = model.Name,
                StopDate = model.ToDate,
                Subject = model.Subject,
                Body = model.Body,
                RelationId = model.RelationId,
                Scheduling = model.RepetitionFrequency,
                AlarmDate = model.FromDate,
                Reciepients = model.Recipients.Select(MapToAdviceUserRelation).ToList()
            };
        }

        private static AdviceUserRelation MapToAdviceUserRelation(RecipientModificationModel model)
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
