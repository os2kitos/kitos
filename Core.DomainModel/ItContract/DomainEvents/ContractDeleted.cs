using System;
using Core.DomainModel.Events;

namespace Core.DomainModel.ItContract.DomainEvents
{
    public class ContractDeleted : IDomainEvent
    {
        public ItContract DeletedContract { get; }

        public ContractDeleted(ItContract deletedContract)
        {
            DeletedContract = deletedContract ?? throw new ArgumentNullException(nameof(deletedContract));
        }
    }
}
