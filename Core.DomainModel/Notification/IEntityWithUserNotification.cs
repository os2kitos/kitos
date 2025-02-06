using Core.DomainModel.References;
using System.Collections.Generic;

namespace Core.DomainModel.Notification
{
    public interface IEntityWithUserNotification : IEntity, IHasUuid
    {
        ReferenceRootType GetRootType();
        ICollection<UserNotification> UserNotifications { get; set; }
    }
}