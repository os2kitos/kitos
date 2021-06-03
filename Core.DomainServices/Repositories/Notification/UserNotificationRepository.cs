using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;
using System.Linq;

namespace Core.DomainServices.Repositories.Notification
{
    public class UserNotificationRepository : IUserNotificationRepository
    {
        private readonly IGenericRepository<UserNotification> _repository;

        public UserNotificationRepository(IGenericRepository<UserNotification> repository)
        {
            _repository = repository;
        }

        public UserNotification Add(UserNotification newNotification)
        {
            var notification = _repository.Insert(newNotification);
            _repository.Save();
            return notification;
        }

        public bool DeleteById(int id)
        {
            var notification = _repository.GetByKey(id);
            if (notification != null)
            {
                _repository.Delete(notification);
                _repository.Save();
                return true;
            }

            return false;
        }

        public Maybe<UserNotification> GetById(int id)
        {
            return _repository.GetByKey(id);
        }

        public IQueryable<UserNotification> GetNotificationFromOrganizationByUserId(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            return _repository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .Where(x => x.ObjectOwnerId == userId)
                .Where(x => x.RelatedEntityType == relatedEntityType);
        }
    }
}
