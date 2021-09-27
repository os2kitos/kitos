using Core.DomainModel.Shared;
using Core.DomainServices.Notifications;
using Core.DomainServices.Repositories.Notification;
using System.Linq;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ContractDeletedUserNotificationsHandler : IDomainEventHandler<EntityDeletedEvent<ItContract>>
    {
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IUserNotificationService _userNotificationService;

        public ContractDeletedUserNotificationsHandler(IUserNotificationRepository userNotificationRepository, IUserNotificationService userNotificationService)
        {
            _userNotificationRepository = userNotificationRepository;
            _userNotificationService = userNotificationService;
        }

        public void Handle(EntityDeletedEvent<ItContract> domainEvent)
        {
            var contractDeleted = domainEvent.Entity;
            var toBeDeleted = _userNotificationRepository.GetByRelatedEntityIdAndType(contractDeleted.Id, RelatedEntityType.itContract).ToList();
            _userNotificationService.BulkDeleteUserNotification(toBeDeleted);
        }
    }
}
