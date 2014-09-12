using Core.DomainModel.ItSystem;

namespace Core.DomainModel.ItContract
{
    public class ItContractItSystemUsage
    {
        public int ItContractId { get; set; } 
        public virtual ItContract ItContract { get; set; }

        public int ItSystemUsageId { get; set; }
        public virtual ItSystemUsage ItSystemUsage { get; set; }
    }
}
