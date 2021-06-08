using Core.DomainModel.ItProject;
using Core.DomainModel.Shared;
using Core.DomainServices.Notifications;
using Core.DomainServices.Repositories.Notification;
using Infrastructure.Services.DomainEvents;
using System.Linq;

namespace Core.ApplicationServices.Model.EventHandler
{
    public class ProjectDeletedUserNotificationsHandler : IDomainEventHandler<EntityDeletedEvent<ItProject>>
    {
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IUserNotificationService _userNotificationService;

        public ProjectDeletedUserNotificationsHandler(IUserNotificationRepository userNotificationRepository, IUserNotificationService userNotificationService)
        {
            _userNotificationRepository = userNotificationRepository;
            _userNotificationService = userNotificationService;
        }

        public void Handle(EntityDeletedEvent<ItProject> domainEvent)
        {
            var projectDeleted = domainEvent.Entity;
            var toBeDeleted = _userNotificationRepository.GetByRelatedEntityIdAndType(projectDeleted.Id, RelatedEntityType.itProject).ToList();
            _userNotificationService.BulkDeleteUserNotification(toBeDeleted);
        }
    }
}
