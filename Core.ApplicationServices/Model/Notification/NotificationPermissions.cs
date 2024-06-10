using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.DomainModel;
using Core.DomainModel.Advice;

namespace Core.ApplicationServices.Model.Notification
{
    public class NotificationPermissions : ResourcePermissionsResult
    {
        public static NotificationPermissions ReadOnly() => new (true, false, false, false);
        private static readonly NotificationPermissions Empty = new(false, false, false, false);

        public bool Deactivate { get; }

        public NotificationPermissions(bool read, bool modify, bool delete, bool deactivate) : base(read, modify, delete)
        {
            Deactivate = deactivate;
        }

        public static Result<NotificationPermissions, OperationError> FromResolutionResult<T>(
            Advice notification,
            T relatedEntity,
            IAuthorizationContext authorizationContext) where T : IEntityWithAdvices
        {
            if (!authorizationContext.AllowReads(relatedEntity))
                return Empty;
            if (!authorizationContext.AllowModify(relatedEntity))
                return ReadOnly();

            var canBeModified = notification.IsActive && notification.AdviceType == AdviceType.Repeat;
            var canBeDeactivated = canBeModified;
            var canBeDeleted = notification.CanBeDeleted;
            return new NotificationPermissions(true, canBeModified, canBeDeleted, canBeDeactivated);
        }
    }
}
