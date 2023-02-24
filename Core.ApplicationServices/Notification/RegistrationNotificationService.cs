﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
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
        private readonly IGenericRepository<Advice> _adviceRepository;
        private readonly IDomainEvents _domainEventHandler;
        private readonly IOperationClock _operationClock;
        private readonly IAdviceRootResolution _adviceRootResolution;

        private readonly Regex _emailValidationRegex = new("([a-zA-Z\\-0-9\\._]+@)([a-zA-Z\\-0-9\\.]+)\\.([a-zA-Z\\-0-9\\.]+)");

        public RegistrationNotificationService(IAdviceService adviceService,
            IAuthorizationContext authorizationContext, 
            ITransactionManager transactionManager,
            IGenericRepository<Advice> adviceRepository,
            IDomainEvents domainEventHandler, 
            IAdviceRootResolution adviceRootResolution, 
            IOperationClock operationClock)
        {
            _adviceService = adviceService;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _adviceRepository = adviceRepository;
            _domainEventHandler = domainEventHandler;
            _adviceRootResolution = adviceRootResolution;
            _operationClock = operationClock;
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

        public Result<Advice, OperationError> GetNotificationById(int id)
        {
            return _adviceService.GetAdviceById(id)
                .Match(AuthorizeReadAccessAndReturnNotification,
                    () => new OperationError($"Notification with id: {id} was not found", OperationFailure.NotFound));
        }

        public IQueryable<AdviceSent> GetSent()
        {
            return _adviceService
                .GetAdvicesAccessibleToCurrentUser()
                .SelectMany(x => x.AdviceSent);
        }

        public Result<Advice, OperationError> CreateImmediateNotification(ImmediateNotificationModel notificationModel)
        {
            var newNotification = MapCreateImmediateModelToEntity(notificationModel);
            return Create(newNotification);
        }

        public Result<Advice, OperationError> CreateScheduledNotification(ScheduledNotificationModel notificationModel)
        {
            var newNotification = MapCreateSchedulingModelToEntity(notificationModel);

            if (newNotification.AlarmDate == null)
            {
                return new OperationError("Start date is not set!", OperationFailure.BadInput);

            }
            if (newNotification.Scheduling is null or Scheduling.Immediate)
            {
                return new OperationError($"Scheduling must be defined and cannot be {nameof(Scheduling.Immediate)} when creating advice of type {nameof(AdviceType.Repeat)}", OperationFailure.BadInput);
            }
            if (newNotification.AlarmDate.Value.Date < _operationClock.Now.Date)
            {
                return new OperationError("Start date is set before today", OperationFailure.BadInput);
            }
            
            if (newNotification.StopDate != null && newNotification.StopDate.Value.Date < newNotification.AlarmDate.Value.Date)
            {
                return new OperationError("Stop date is set before Start date", OperationFailure.BadInput);
            }
            
            return Create(newNotification);
        }

        public Result<Advice, OperationError> Update(int notificationId, UpdateScheduledNotificationModel notificationModel)
        {
            using var transaction = _transactionManager.Begin();

            var entityResult = GetNotificationById(notificationId);
            if (entityResult.Failed)
                return entityResult.Error;
            var entity = entityResult.Value;
            if (!entity.IsActive)
                return new OperationError("Cannot update inactive advice", OperationFailure.BadInput);
            if (entity.AdviceType == AdviceType.Immediate)
                return new OperationError("Immediate notification cannot be updated!", OperationFailure.BadInput);
            if (!_authorizationContext.AllowModify(ResolveRoot(entity)))
                return new OperationError($"User is not allowed to modify notification with id: {notificationId}", OperationFailure.Forbidden);

            MapUpdateSchedulingModelToEntity(notificationModel, entity);
            
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

            if (!_authorizationContext.AllowModify(ResolveRoot(entity)))
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

            if (!_authorizationContext.AllowModify(ResolveRoot(entity)))
            {
                return new OperationError($"User is not allowed to deactivate notification with id: {adviceId}", OperationFailure.Forbidden);
            }
            _adviceService.Deactivate(entity);
            RaiseAsRootModification(entity);
            transaction.Commit();

            return entity;
        }

        private Result<Advice, OperationError> Create(Advice newNotification)
        {
            if (!_authorizationContext.AllowModify(ResolveRoot(newNotification)))
            {
                return new OperationError($"User is not allowed to create notification for root type with id: {newNotification.RelationId}", OperationFailure.Forbidden);
            }

            var res = newNotification.Reciepients.Where(x => x.RecpientType == RecipientType.USER).ToList();
            var res2 = res.Any(x => !_emailValidationRegex.IsMatch(x.Email));

            if (newNotification.Reciepients.Where(x => x.RecpientType == RecipientType.USER).Any(x => !_emailValidationRegex.IsMatch(x.Email)))
            {
                return new OperationError("Invalid email exists among receivers or CCs", OperationFailure.BadInput);
            }

            using var transaction = _transactionManager.Begin();

            newNotification.IsActive = true;

            _adviceRepository.Insert(newNotification);
            _domainEventHandler.Raise(new EntityCreatedEvent<Advice>(newNotification));
            _adviceRepository.Save();

            var name = Advice.CreateJobId(newNotification.Id);

            newNotification.JobId = name;

            //Sends a notification and updates the job id
            _adviceService.CreateAdvice(newNotification);

            transaction.Commit();

            return newNotification;
        }

        private Result<Advice, OperationError> AuthorizeReadAccessAndReturnNotification(Advice notification)
        {
            return _authorizationContext.AllowReads(ResolveRoot(notification))
                ? notification
                : new OperationError("User is not allowed to read the notification", OperationFailure.Forbidden);
        }

        private static void MapUpdateSchedulingModelToEntity(UpdateScheduledNotificationModel model, Advice notification)
        {
            MapBasePropertiesWithNameAndStopDate(model, notification);
        }

        private static Advice MapCreateSchedulingModelToEntity(ScheduledNotificationModel model)
        {
            var notification = new Advice();
            MapBasePropertiesWithNameAndStopDate(model, notification);
            MapRecipients(model, notification);
            notification.AlarmDate = model.FromDate;
            notification.Scheduling = model.RepetitionFrequency;

            return notification;
        }

        private static Advice MapCreateImmediateModelToEntity(ImmediateNotificationModel model)
        {
            var notification = new Advice();
            MapBasePropertiesToNotification(model, notification);
            MapRecipients(model, notification);

            notification.Scheduling = Scheduling.Immediate;
            notification.StopDate = null;
            notification.AlarmDate = null;

            return notification;
        }

        private static void MapRecipients<T>(T model, Advice notification) where T : class, IHasBaseNotificationPropertiesModel, IHasRecipientModels
        {
            var recipients = MapRecipientModelToRelation(model.Ccs, RecieverType.CC, model.BaseProperties.Type).ToList();
            var joinedRecipients = recipients.Concat(MapRecipientModelToRelation(model.Receivers, RecieverType.RECIEVER, model.BaseProperties.Type)).ToList();
            notification.Reciepients = joinedRecipients;
        }

        private static void MapBasePropertiesWithNameAndStopDate<T>(T model, Advice notification)
            where T : class, IHasName, IHasToDate, IHasBaseNotificationPropertiesModel
        {
            MapBasePropertiesToNotification(model, notification);
            notification.Name = model.Name;
            notification.StopDate = model.ToDate;
        }

        private static void MapBasePropertiesToNotification<T>(T model, Advice notification) where T: class, IHasBaseNotificationPropertiesModel
        {
            notification.Subject = model.BaseProperties.Subject;
            notification.Body = model.BaseProperties.Body;
            notification.RelationId = model.BaseProperties.RelationId;
            notification.Type = model.BaseProperties.Type;
            notification.AdviceType = model.BaseProperties.AdviceType;
        }

        private static IEnumerable<AdviceUserRelation> MapRecipientModelToRelation(RecipientModel model, RecieverType receiverType, RelatedEntityType relatedEntityType)
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
                RecpientType = RecipientType.ROLE,
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
