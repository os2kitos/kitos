using System;

namespace Core.DomainServices.Repositories.KLE
{
    public class OperationClock : IOperationClock
    {
        public DateTime Now => DateTime.Now;
    }
}