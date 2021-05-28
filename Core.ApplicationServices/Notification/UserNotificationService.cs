using Core.ApplicationServices.Authorization;
using Core.DomainModel.Advice;
using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Notification;
using Infrastructure.Services.DataAccess;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Core.ApplicationServices.Notification
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly ITransactionManager _transactionManager; 
        private readonly IAuthorizationContext _authorizationContext;

        public UserNotificationService(IUserNotificationRepository userNotificationRepository, ITransactionManager transactionManager, IAuthorizationContext authorizationContext)
        {
            _userNotificationRepository = userNotificationRepository;
            _transactionManager = transactionManager;
            _authorizationContext = authorizationContext;
        }

        public Result<UserNotification, OperationError> AddUserNotification(int userToNotifyId, string name, string message, int relatedEntityId, ObjectType relatedEntityType, NotificationType notificationType)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.Serializable);

            var notification = new UserNotification
            {
                Name = name,
                NotificationMessage = message,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                ObjectOwnerId = userToNotifyId,
                NotificationType = notificationType
            };

            var userNotification = _userNotificationRepository.Add(notification);
            transaction.Commit();
            return userNotification;
        }

        public Result<UserNotification, OperationError> Delete(int id)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.Serializable);

            var result = _userNotificationRepository.GetById(id);

            if (result.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var notificationToDelete = result.Value;

            if (!_authorizationContext.AllowDelete(notificationToDelete))
                return new OperationError(OperationFailure.Forbidden);

            _userNotificationRepository.DeleteById(id);
            transaction.Commit();
            return notificationToDelete;
        }

        public Result<IEnumerable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId)
        {
            return _userNotificationRepository.GetNotificationFromOrganizationByUserId(organizationId, userId).ToList();
        }
    }
}
