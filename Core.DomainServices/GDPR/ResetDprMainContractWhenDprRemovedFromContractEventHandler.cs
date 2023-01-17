using Core.Abstractions.Extensions;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Events;
using Core.DomainModel.ItContract;

namespace Core.DomainServices.GDPR
{
    public class ResetDprMainContractWhenDprRemovedFromContractEventHandler : IDomainEventHandler<DataProcessingRegistrationRemovedFromItContractEvent>
    {
        public void Handle(DataProcessingRegistrationRemovedFromItContractEvent domainEvent)
        {
            var dpr = domainEvent.DataProcessingRegistration;
            var contract = domainEvent.ItContract;

            if (ShouldReset(dpr, contract))
            {
                dpr.ResetMainContract();
            }
        }

        private static bool ShouldReset(DataProcessingRegistration dpr, ItContract contract)
        {
            return dpr.MainContract
                .FromNullable()
                .Select(existingMainContract => existingMainContract == contract)
                .GetValueOrFallback(false);
        }
    }
}
