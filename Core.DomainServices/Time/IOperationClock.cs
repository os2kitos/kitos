using System;

namespace Core.DomainServices.Time
{
    public interface IOperationClock
    {
        DateTime Now { get; }
    }
}