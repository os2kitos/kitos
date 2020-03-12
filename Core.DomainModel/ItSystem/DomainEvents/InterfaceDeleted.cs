using System;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainModel.ItSystem.DomainEvents
{
    public class InterfaceDeleted : IDomainEvent
    {
        public ItInterface DeletedInterface { get; }

        public InterfaceDeleted(ItInterface deletedInterface)
        {
            DeletedInterface = deletedInterface ?? throw new ArgumentNullException(nameof(deletedInterface));
        }
    }
}
