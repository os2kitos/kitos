using Core.ApplicationServices.Authorization;
using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Repositories.Notification;
using Infrastructure.Services.DataAccess;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Core.ApplicationServices.Notification
{
    //TODO: 1: Remove auth check and move this service to "domain services".
    //TODO: 2: Introduce a new IUserNotificationApplicationService which adds authorization before calling the domain service
    //TODO: 3: Make the attribute use the domain service since that is the system working, not the user...
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

        public Result<UserNotification, OperationError> AddUserNotification(int organizationId, int userToNotifyId, string name, string message, int relatedEntityId, RelatedEntityType relatedEntityType, NotificationType notificationType)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);
            //TODO: Validate that the relation actually points to an object in the database :-)
            var notification = new UserNotification
            {
                Name = name,
                NotificationMessage = message,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                ObjectOwnerId = userToNotifyId,
                NotificationType = notificationType,
                OrganizationId = organizationId
            };

            var userNotification = _userNotificationRepository.Add(notification);
            transaction.Commit();
            return userNotification;
        }

        public Result<UserNotification, OperationError> Delete(int id)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            var result = _userNotificationRepository.GetById(id);

            if (result.IsNone)
            {
                transaction.Rollback();
                return new OperationError(OperationFailure.NotFound);
            }

            var notificationToDelete = result.Value;

            if (!_authorizationContext.AllowDelete(notificationToDelete))
            {
                transaction.Rollback();
                return new OperationError(OperationFailure.Forbidden);
            }

            _userNotificationRepository.DeleteById(id);
            transaction.Commit();
            return notificationToDelete;
        }

        public Result<IEnumerable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            return _userNotificationRepository.GetNotificationFromOrganizationByUserId(organizationId, userId, relatedEntityType).ToList();
        }

        public Result<int, OperationError> GetNumberOfUnresolvedNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            return _userNotificationRepository.GetNotificationFromOrganizationByUserId(organizationId, userId, relatedEntityType).ToList().Count();
        }
    }
}
