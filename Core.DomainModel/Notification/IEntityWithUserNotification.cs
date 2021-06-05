using Core.DomainModel.References;

namespace Core.DomainModel.Notification
{
    public interface IEntityWithUserNotification : IEntity
    {
        ReferenceRootType GetRootType();
    }
}