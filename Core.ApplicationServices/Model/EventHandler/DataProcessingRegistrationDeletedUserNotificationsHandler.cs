using Core.DomainModel.GDPR;
using Core.DomainModel.Shared;
using Core.DomainServices.Notifications;
using Core.DomainServices.Repositories.Notification;
using Infrastructure.Services.DomainEvents;
using System.Linq;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class DataProcessingRegistrationDeletedUserNotificationsHandler : IDomainEventHandler<EntityDeletedEvent<DataProcessingRegistration>>
    {
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IUserNotificationService _userNotificationService;

        public DataProcessingRegistrationDeletedUserNotificationsHandler(IUserNotificationRepository userNotificationRepository, IUserNotificationService userNotificationService)
        {
            _userNotificationRepository = userNotificationRepository;
            _userNotificationService = userNotificationService;
        }

        public void Handle(EntityDeletedEvent<DataProcessingRegistration> domainEvent)
        {
            var dataProcessingRegistration = domainEvent.Entity;
            var toBeDeleted = _userNotificationRepository.GetByRelatedEntityIdAndType(dataProcessingRegistration.Id, RelatedEntityType.dataProcessingRegistration).ToList();
            _userNotificationService.BulkDeleteUserNotification(toBeDeleted);
        }
    }
}
