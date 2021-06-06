using Core.DomainModel.Notification;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;
using System;
using System.Collections.Generic;
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

        public IEnumerable<UserNotification> GetByRelatedEntityIdAndType(int relatedEntityId, RelatedEntityType relatedEntityType)
        {
            switch (relatedEntityType)
            {
                case RelatedEntityType.itContract:
                    return _repository.Get(x => x.Itcontract_Id == relatedEntityId);
                case RelatedEntityType.itProject:
                    return _repository.Get(x => x.ItProject_Id == relatedEntityId);
                case RelatedEntityType.itSystemUsage:
                    return _repository.Get(x => x.ItSystemUsage_Id == relatedEntityId);
                case RelatedEntityType.dataProcessingRegistration:
                    return _repository.Get(x => x.DataProcessingRegistration_Id == relatedEntityId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(relatedEntityType));
            }
        }

        public IQueryable<UserNotification> GetNotificationFromOrganizationByUserId(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            var query = _repository
                .AsQueryable()
                .ByOrganizationId(organizationId)
                .Where(x => x.NotificationRecipientId == userId);
            switch (relatedEntityType)
            {
                case RelatedEntityType.itContract:
                    return query.Where(x => x.Itcontract_Id != null);
                case RelatedEntityType.itProject:
                    return query.Where(x => x.ItProject_Id != null);
                case RelatedEntityType.itSystemUsage:
                    return query.Where(x => x.ItSystemUsage_Id != null);
                case RelatedEntityType.dataProcessingRegistration:
                    return query.Where(x => x.DataProcessingRegistration_Id != null);
                default:
                    throw new ArgumentOutOfRangeException(nameof(relatedEntityType));
            }
        }
    }
}
