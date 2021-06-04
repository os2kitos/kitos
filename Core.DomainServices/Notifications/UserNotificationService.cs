using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Notification;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.SystemUsage;
using Infrastructure.Services.DataAccess;
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

        public UserNotificationService(
            IUserNotificationRepository userNotificationRepository, 
            ITransactionManager transactionManager,
            IItSystemUsageRepository systemUsageRepository,
            IItContractRepository contractRepository,
            IItProjectRepository projectRepository,
            IDataProcessingRegistrationRepository dataProcessingRepository)
        {
            _userNotificationRepository = userNotificationRepository;
            _transactionManager = transactionManager;
            _systemUsageRepository = systemUsageRepository;
            _contractRepository = contractRepository;
            _projectRepository = projectRepository;
            _dataProcessingRepository = dataProcessingRepository;
        }

        public Result<UserNotification, OperationError> AddUserNotification(int organizationId, int userToNotifyId, string name, string message, int relatedEntityId, RelatedEntityType relatedEntityType, NotificationType notificationType)
        {

            if (!RelatedEntityExists(relatedEntityId, relatedEntityType))
            {
                return new OperationError(OperationFailure.NotFound);
            }

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            var notification = new UserNotification(name, message, notificationType, organizationId, userToNotifyId);

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

        public bool Delete(int id)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            var deleted = _userNotificationRepository.DeleteById(id);
            transaction.Commit();
            return deleted;
        }

        public Result<UserNotification, OperationError> GetUserNotification(int id)
        {
            var result = _userNotificationRepository.GetById(id);
            if (result.IsNone)
            {
                return new OperationError(OperationFailure.NotFound);
            }
            return result.Value;
        }

        public Result<IEnumerable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            return _userNotificationRepository.GetNotificationFromOrganizationByUserId(organizationId, userId, relatedEntityType).ToList();
        }

        public Result<int, OperationError> GetNumberOfUnresolvedNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            return _userNotificationRepository.GetNotificationFromOrganizationByUserId(organizationId, userId, relatedEntityType).ToList().Count();
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
                    return dataProcessingRegistrationExists != null;
                default:
                    return false;
            }
        }
    }
}
