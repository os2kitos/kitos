using Core.DomainModel.Events;

namespace Core.DomainModel.Organization.DomainEvents
{
    public class AdministrativeAccessRightsChanged : IDomainEvent
    {
        public int UserId { get; }

        public AdministrativeAccessRightsChanged(int userId)
        {
            UserId = userId;
        }
    }
}
