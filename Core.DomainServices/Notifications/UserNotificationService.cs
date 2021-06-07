using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Notification;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.SystemUsage;
using Core.DomainServices.Time;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Core.DomainServices.Notifications
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly IItSystemUsageRepository _systemUsageRepository;
        private readonly IItContractRepository _contractRepository; 
        private readonly IItProjectRepository _projectRepository; 
        private readonly IDataProcessingRegistrationRepository _dataProcessingRepository;

        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IOperationClock _operationClock;
        private readonly ILogger _logger;

        public UserNotificationService(
            IUserNotificationRepository userNotificationRepository,
            ITransactionManager transactionManager,
            IItSystemUsageRepository systemUsageRepository,
            IItContractRepository contractRepository,
            IItProjectRepository projectRepository,
            IDataProcessingRegistrationRepository dataProcessingRepository,
            IOperationClock operationClock, 
            ILogger logger)
        {
            _userNotificationRepository = userNotificationRepository;
            _transactionManager = transactionManager;
            _systemUsageRepository = systemUsageRepository;
            _contractRepository = contractRepository;
            _projectRepository = projectRepository;
            _dataProcessingRepository = dataProcessingRepository;
            _operationClock = operationClock;
            _logger = logger;
        }

        public Result<UserNotification, OperationError> AddUserNotification(int organizationId, int userToNotifyId, string name, string message, int relatedEntityId, RelatedEntityType relatedEntityType, NotificationType notificationType)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            if (!RelatedEntityExists(relatedEntityId, relatedEntityType))
            {
                transaction.Rollback();
                return new OperationError(OperationFailure.NotFound);
            }

            var notification = new UserNotification(name, message, notificationType, organizationId, userToNotifyId, _operationClock.Now);

            switch (relatedEntityType)
            {
                case RelatedEntityType.itContract:
                    notification.Itcontract_Id = relatedEntityId;
                    break;
                case RelatedEntityType.itProject:
                    notification.ItProject_Id = relatedEntityId;
                    break;
                case RelatedEntityType.itSystemUsage:
                    notification.ItSystemUsage_Id = relatedEntityId;
                    break;
                case RelatedEntityType.dataProcessingRegistration:
                    notification.DataProcessingRegistration_Id = relatedEntityId;
                    break;
                default:
                    return new OperationError(OperationFailure.BadInput);
            }


            var userNotification = _userNotificationRepository.Add(notification);
            transaction.Commit();
            return userNotification;
        }

        public Maybe<OperationError> Delete(int id)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            var deleted = _userNotificationRepository.DeleteById(id);
            transaction.Commit();
            return deleted;
        }

        public Result<UserNotification, OperationError> GetUserNotification(int id)
        {
            return _userNotificationRepository.GetById(id).Match<Result<UserNotification, OperationError>>(result => result, () => new OperationError(OperationFailure.NotFound));
        }

        public Result<IQueryable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            return Result<IQueryable<UserNotification>, OperationError>.Success(_userNotificationRepository.GetNotificationFromOrganizationByUserId(organizationId, userId, relatedEntityType));
        }


        private bool RelatedEntityExists(int relatedEntityId, RelatedEntityType relatedEntityType)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.itContract:
                    var contractExists = _contractRepository.GetById(relatedEntityId);
                    return contractExists != null;
                case RelatedEntityType.itProject:
                    var projectExists = _projectRepository.GetById(relatedEntityId);
                    return projectExists != null;
                case RelatedEntityType.itSystemUsage:
                    var systemUsageExists = _systemUsageRepository.GetSystemUsage(relatedEntityId);
                    return systemUsageExists != null;
                case RelatedEntityType.dataProcessingRegistration:
                    var dataProcessingRegistrationExists = _dataProcessingRepository.GetById(relatedEntityId);
                    return dataProcessingRegistrationExists.HasValue;
                default:
                    return false;
            }
        }

        public void BulkDeleteUserNotification(IEnumerable<UserNotification> toBeDeleted)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            foreach (var userNotification in toBeDeleted)
            {
                var deleteResult = _userNotificationRepository.DeleteById(userNotification.Id);
                if (deleteResult.HasValue)
                {
                    transaction.Rollback();
                    _logger.Error($"Failed to do bulk user notification deletion. Failed on user notification with Id: {userNotification.Id}. With failure: {deleteResult.Value}");
                }
            }
            transaction.Commit();
        }
    }
}
