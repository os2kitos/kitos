using Core.DomainModel.Events;
using Core.DomainModel.GDPR.Events;

namespace Core.DomainServices.GDPR
{
    internal class DataProcessingRegistrationRemovedFromItContractEventHandler : IDomainEventHandler<DataProcessingRegistrationRemovedFromItContractEvent>
    {
        public void Handle(DataProcessingRegistrationRemovedFromItContractEvent domainEvent)
        {
            var dpr = domainEvent.Entity;
            var contract = domainEvent.ItContract;
            if(contract.Id != dpr.MainContract?.Id)
                return;

            dpr.ResetMainContract();
        }
    }
}
