using System;

namespace Core.DomainServices.Repositories.KLE
{
    public interface IOperationClock
    {
        DateTime Now { get; }
    }
}