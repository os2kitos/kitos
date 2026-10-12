using System;

namespace Core.DomainModel.Events
{
    public class UserLoggedInEvent(Guid userUuid) : IDomainEvent
    {
        public Guid UserUuid { get; private set; } = userUuid;
    }
}
