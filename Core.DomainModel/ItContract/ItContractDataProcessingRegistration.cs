using Core.DomainModel.GDPR;

namespace Core.DomainModel.ItContract
{
    public class ItContractDataProcessingRegistration
    {
        public int ItContractId { get; set; }
        public virtual ItContract ItContract{ get; set; }

        public int DataProcessingRegistrationId { get; set; }
        public virtual DataProcessingRegistration DataProcessingRegistration { get; set; }
    }
}
