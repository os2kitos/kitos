using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Core.DomainServices.Extensions;

using System;
using System.Linq;
using Core.Abstractions.Types;

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

        public Maybe<OperationError> DeleteById(int id)
        {
            var notification = _repository.GetByKey(id);
            if (notification != null)
            {
                _repository.Delete(notification);
                _repository.Save();
                return Maybe<OperationError>.None;
            }

            return new OperationError(OperationFailure.UnknownError);
        }

        public Maybe<UserNotification> GetById(int id)
        {
            return _repository.GetByKey(id);
        }

        public IQueryable<UserNotification> GetByRelatedEntityIdAndType(int relatedEntityId, RelatedEntityType relatedEntityType)
        {
            return relatedEntityType switch
            {
                RelatedEntityType.itContract => _repository.AsQueryable()
                    .Where(x => x.Itcontract_Id == relatedEntityId),
                RelatedEntityType.itSystemUsage => _repository.AsQueryable()
                    .Where(x => x.ItSystemUsage_Id == relatedEntityId),
                RelatedEntityType.dataProcessingRegistration => _repository.AsQueryable()
                    .Where(x => x.DataProcessingRegistration_Id == relatedEntityId),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType))
            };
        }

        public IQueryable<UserNotification> GetNotificationFromOrganizationByUserId(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            var query = _repository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .Where(x => x.NotificationRecipientId == userId);
            
            return relatedEntityType switch
            {
                RelatedEntityType.itContract => query.Where(x => x.Itcontract_Id != null),
                RelatedEntityType.itSystemUsage => query.Where(x => x.ItSystemUsage_Id != null),
                RelatedEntityType.dataProcessingRegistration => query.Where(
                    x => x.DataProcessingRegistration_Id != null),
                _ => throw new ArgumentOutOfRangeException(nameof(relatedEntityType))
            };
        }
    }
}
