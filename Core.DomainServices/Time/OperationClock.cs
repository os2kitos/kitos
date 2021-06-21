using System;

namespace Core.DomainServices.Time
{
    public class OperationClock : IOperationClock
    {
        public DateTime Now => DateTime.UtcNow; //We should always use utc since that's the default from the db
    }
}