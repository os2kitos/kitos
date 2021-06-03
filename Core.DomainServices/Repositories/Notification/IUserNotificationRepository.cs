using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Infrastructure.Services.Types;
using System.Linq;

namespace Core.DomainServices.Repositories.Notification
{
    public interface IUserNotificationRepository
    {
        UserNotification Add(UserNotification newNotification);
        bool DeleteById(int id);
        Maybe<UserNotification> GetById(int id);
        IQueryable<UserNotification> GetNotificationFromOrganizationByUserId(int organizationId, int userId, RelatedEntityType relatedEntityType);
    }
}
