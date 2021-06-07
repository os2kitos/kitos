using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.Shared;
using Core.DomainServices.Notifications;
using Core.DomainServices.Repositories.Notification;
using Infrastructure.Services.DomainEvents;
using System.Linq;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ContractDeletedUserNotificationsHandler : IDomainEventHandler<ContractDeleted>
    {
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IUserNotificationService _userNotificationService;

        public ContractDeletedUserNotificationsHandler(IUserNotificationRepository userNotificationRepository, IUserNotificationService userNotificationService)
        {
            _userNotificationRepository = userNotificationRepository;
            _userNotificationService = userNotificationService;
        }

        public void Handle(ContractDeleted domainEvent)
        {
            var contractDeleted = domainEvent.DeletedContract;
            var toBeDeleted = _userNotificationRepository.GetByRelatedEntityIdAndType(contractDeleted.Id, RelatedEntityType.itContract).ToList();
            _userNotificationService.BulkDeleteUserNotification(toBeDeleted);
        }
    }
}
