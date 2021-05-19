using Core.DomainModel.Advice;
using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainServices;
using Infrastructure.Services.DataAccess;
using System.Data;

namespace Core.ApplicationServices.Notification
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly IGenericRepository<UserNotification> _userNotificationRepository;
        private readonly ITransactionManager _transactionManager;

        public UserNotificationService(IGenericRepository<UserNotification> userNotificationRepository, ITransactionManager transactionManager)
        {
            _userNotificationRepository = userNotificationRepository;
            _transactionManager = transactionManager;
        }

        public Result<UserNotification, OperationError> AddUserNotification(int userToNotifyId, string name, string message, int relatedEntityId, ObjectType relatedEntityType)
        {
            using var transaction = _transactionManager.Begin(IsolationLevel.Serializable);

            var notification = new UserNotification
            {
                Name = name,
                NotificationMessage = message,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                ObjectOwnerId = userToNotifyId
            };

            var userNotification = _userNotificationRepository.Insert(notification);
            _userNotificationRepository.Save();
            transaction.Commit();
            return userNotification;
        }
    }
}
