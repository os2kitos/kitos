using System;

namespace Core.DomainModel.ItContract
{
    public class HandoverTrial : Entity
    {
        public DateTime? Expected { get; set; }
        public DateTime? Approved { get; set; }
        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }
        
        public int? HandoverTrialTypeId { get; set; }
        public virtual HandoverTrialType HandoverTrialType { get; set; }
    }
}