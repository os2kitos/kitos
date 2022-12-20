using Core.DomainModel.Events;

namespace Core.DomainModel.GDPR.Events
{
    public class DataProcessingRegistrationRemovedFromItContractEvent : IDomainEvent
    {
        public DataProcessingRegistrationRemovedFromItContractEvent(DataProcessingRegistration dataProcessingRegistration, ItContract.ItContract itContract)
        {
            DataProcessingRegistration = dataProcessingRegistration;
            ItContract = itContract;
        }

        public DataProcessingRegistration DataProcessingRegistration { get; }
        public ItContract.ItContract ItContract { get; }
    }
}
