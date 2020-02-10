using System;
using Infrastructure.Services.DomainEvents;

namespace Core.DomainModel.ItSystemUsage.DomainEvents
{
    public class SystemUsageDeleted : IDomainEvent
    {
        public ItSystemUsage DeletedSystemUsage { get; }

        public SystemUsageDeleted(ItSystemUsage deletedSystemUsage)
        {
            DeletedSystemUsage = deletedSystemUsage ?? throw new ArgumentNullException();
        }
    }
}
