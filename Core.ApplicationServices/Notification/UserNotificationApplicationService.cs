using Core.ApplicationServices.Authorization;
using Core.DomainModel.Notification;
using Core.DomainModel.Shared;
using Core.DomainServices.Notifications;

using System.Linq;
using Core.Abstractions.Types;
using System;
using Core.DomainServices.Generic;
using Core.DomainModel.Organization;
using Core.DomainModel;

namespace Core.ApplicationServices.Notification
{
    public class UserNotificationApplicationService : IUserNotificationApplicationService
    {
        private readonly IOrganizationalUserContext _activeUserContext;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;

        public UserNotificationApplicationService(IUserNotificationService userNotificationService, IOrganizationalUserContext activeUserContext, IEntityIdentityResolver entityIdentityResolver)
        {
            _userNotificationService = userNotificationService;
            _activeUserContext = activeUserContext;
            _entityIdentityResolver = entityIdentityResolver;
        }

        public Maybe<OperationError> Delete(int id)
        {
            var getResult = _userNotificationService.GetUserNotification(id);

            if (getResult.Failed)
            {
                return getResult.Error;
            }

            var notificationToDelete = getResult.Value;

            if (!IsActiveUser(notificationToDelete.NotificationRecipientId))
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return _userNotificationService.Delete(id);
        }

        public Result<IQueryable<UserNotification>, OperationError> GetNotificationsForUser(int organizationId, int userId, RelatedEntityType relatedEntityType)
        {
            if (!IsActiveUser(userId))
                return new OperationError(OperationFailure.Forbidden);

            return _userNotificationService.GetNotificationsForUser(organizationId, userId, relatedEntityType);
        }

        public Maybe<OperationError> DeleteByUuid(Guid notificationUuid)
        {
            return _entityIdentityResolver.ResolveDbId<UserNotification>(notificationUuid)
                .Match(Delete, () => new OperationError($"Could not find notification with uuid: {notificationUuid}", OperationFailure.NotFound));
        }

        public Result<IQueryable<UserNotification>, OperationError> GetNotificationsForUserByUuid(Guid organizationUuid, Guid userUuid, RelatedEntityType relatedEntityType)
        {
            var orgIdResult = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            if (orgIdResult.IsNone)
            {
                return new OperationError($"Could not find organization with uuid: {organizationUuid}", OperationFailure.NotFound);
            }
            return _entityIdentityResolver.ResolveDbId<User>(userUuid)
                .Match(
                    userId => GetNotificationsForUser(orgIdResult.Value, userId, relatedEntityType),
                    () => new OperationError($"Could not find user with uuid: {organizationUuid}", OperationFailure.NotFound));
        }

        private bool IsActiveUser(int userId)
        {
            return userId == _activeUserContext.UserId;
        }
    }
}
