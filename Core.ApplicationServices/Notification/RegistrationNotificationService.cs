using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Notification;
using Core.ApplicationServices.Model.Notification.Write;
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

        public Result<IQueryable<Advice>, OperationError> GetNotificationsByOrganizationId(int organizationId)
        {
            return _authorizationContext.GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All 
                ? new OperationError($"User is not allowed to read organization with id: {organizationId}", OperationFailure.Forbidden) 
                : Result<IQueryable<Advice>, OperationError>.Success(_adviceService.GetAdvicesForOrg(organizationId));
        }

        public Result<Advice, OperationError> GetNotificationById(int id)
        {
            return _adviceService.GetAdviceById(id)
                .Match(WithReadAccess,
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
            
            if (newNotification.Scheduling == Scheduling.Immediate)
            {
                return new OperationError($"Scheduling must be defined and cannot be {nameof(Scheduling.Immediate)} when creating advice of type {nameof(AdviceType.Repeat)}", OperationFailure.BadInput);
            }

            var startDate = newNotification.AlarmDate.GetValueOrDefault(DateTime.MinValue).Date;
            if (startDate < _operationClock.Now.Date)
            {
                return new OperationError("Start date is set before today", OperationFailure.BadInput);
            }
            
            if (newNotification.StopDate != null && newNotification.StopDate.Value.Date < startDate)
            {
                return new OperationError("Stop date is set before Start date", OperationFailure.BadInput);
            }
            
            return Create(newNotification);
        }

        public Result<Advice, OperationError> Update(int notificationId, UpdateScheduledNotificationModel notificationModel)
        {
            var entityResult = GetNotificationById(notificationId).Bind(WithModifyAccess);
            if (entityResult.Failed)
                return entityResult.Error;
            var entity = entityResult.Value;
            if (!entity.IsActive)
                return new OperationError("Cannot update inactive advice", OperationFailure.BadInput);
            if (entity.AdviceType == AdviceType.Immediate)
                return new OperationError("Immediate notification cannot be updated!", OperationFailure.BadInput);

            using var transaction = _transactionManager.Begin();

            MapUpdateSchedulingModelToEntity(notificationModel, entity);
            
            _adviceRepository.Update(entity);
            var error = RaiseAsRootModification(entity);

            if (error.HasValue)
            {
                transaction.Rollback();
                return error.Value;
            }

            _adviceRepository.Save();

            _adviceService.UpdateSchedule(entity);

            transaction.Commit();

            return entity;
        }

        public Maybe<OperationError> Delete(int notificationId)
        {
            var entityResult = GetNotificationById(notificationId).Bind(WithModifyAccess);
            if (entityResult.Failed)
            {
                return entityResult.Error;
            }

            var entity = entityResult.Value;

            if (!entity.CanBeDeleted)
            {
                return new OperationError("Cannot delete advice which is active or has been sent", OperationFailure.BadState);
            }

            using var transaction = _transactionManager.Begin();

            _adviceService.Delete(entity);
            var error = RaiseAsRootModification(entity);

            if (error.HasValue)
            {
                transaction.Rollback();
                return error.Value;
            }
            
            transaction.Commit();

            return Maybe<OperationError>.None;
        }

        public Result<Advice, OperationError> DeactivateNotification(int notificationId)
        {
            var entityResult = GetNotificationById(notificationId).Bind(WithModifyAccess);
            if (entityResult.Failed)
            {
                return entityResult.Error;
            }
            var entity = entityResult.Value;

            using var transaction = _transactionManager.Begin();

            _adviceService.Deactivate(entity);

            var error = RaiseAsRootModification(entity);

            if (error.HasValue)
            {
                transaction.Rollback();
                return error.Value;
            }

            transaction.Commit();

            return entity;
        }

        private Result<Advice, OperationError> Create(Advice newNotification)
        {
            var validationError = WithModifyAccess(newNotification);
            if (validationError.Failed)
            {
                return validationError.Error;
            }

            if (newNotification.Reciepients.Where(x => x.RecpientType == RecipientType.USER).Any(x => !_emailValidationRegex.IsMatch(x.Email)))
            {
                return new OperationError("Invalid email exists among receivers or CCs", OperationFailure.BadInput);
            }

            using var transaction = _transactionManager.Begin();

            newNotification.IsActive = true;

            _adviceRepository.Insert(newNotification);
            var error = RaiseAsRootModification(newNotification);

            if (error.HasValue)
            {
                transaction.Rollback();
                return error.Value;
            }

            _adviceRepository.Save();
            transaction.Commit();

            var name = Advice.CreateJobId(newNotification.Id);

            newNotification.JobId = name;

            //Sends a notification and updates the job id
            _adviceService.CreateAdvice(newNotification);


            return newNotification;
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
            notification.Name = "Ikke navngivet";
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
            where T : class, IHasReadonlyName, IHasReadonlyToDate, IHasBaseNotificationPropertiesModel
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

        private Result<Advice, OperationError> WithReadAccess(Advice notification)
        {
            return ResolveRoot(notification)
                .Bind(root => _authorizationContext.AllowReads(root)
                    ? Result<Advice, OperationError>.Success(notification)
                    : new OperationError("User is not allowed to read the notification", OperationFailure.Forbidden));
        }

        private Result<Advice,OperationError> WithModifyAccess(Advice notification)
        {
            return ResolveRoot(notification)
                .Match(root => _authorizationContext.AllowModify(root) 
                    ? Result<Advice, OperationError>.Success(notification)
                    : new OperationError($"User is not allowed to modify notification with id: {notification.Id}", OperationFailure.Forbidden),
                    error => error);
        }

        private Result<IEntityWithAdvices, OperationError> ResolveRoot(Advice notification)
        {
            return _adviceRootResolution.Resolve(notification)
                .Match(Result<IEntityWithAdvices, OperationError>.Success,
                    () => new OperationError($"Root entity for notification with id: {notification.Id} was not found", OperationFailure.NotFound));
        }
 
        private Maybe<OperationError> RaiseAsRootModification(Advice entity)
        {
            return ResolveRoot(entity)
                .Match(root =>
                    {
                        switch (root)
                        {
                            case ItContract contract:
                                _domainEventHandler.Raise(new EntityUpdatedEvent<ItContract>(contract));
                                break;
                            case ItSystemUsage usage:
                                _domainEventHandler.Raise(new EntityUpdatedEvent<ItSystemUsage>(usage));
                                break;
                            case DataProcessingRegistration registration:
                                _domainEventHandler.Raise(new EntityUpdatedEvent<DataProcessingRegistration>(registration));
                                break;
                        }

                        return Maybe<OperationError>.None;
                    }, error => error);
        }
    }
}
