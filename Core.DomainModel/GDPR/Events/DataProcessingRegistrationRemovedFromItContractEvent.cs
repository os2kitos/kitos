using Core.DomainModel.Events;
using Core.DomainModel.Shared;

namespace Core.DomainModel.GDPR.Events
{
    public class DataProcessingRegistrationRemovedFromItContractEvent : EntityLifeCycleEvent<DataProcessingRegistration>
    {
        public DataProcessingRegistrationRemovedFromItContractEvent(DataProcessingRegistration entity, ItContract.ItContract itContract) : base(LifeCycleEventType.Deleting, entity)
        {
            ItContract = itContract;
        }

        public ItContract.ItContract ItContract { get; set; }
    }
}
