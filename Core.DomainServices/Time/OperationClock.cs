using System;

namespace Core.DomainServices.Time
{
    public class OperationClock : IOperationClock
    {
        public DateTime Now => DateTime.Now;
    }
}