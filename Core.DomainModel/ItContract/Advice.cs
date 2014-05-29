using System;

namespace Core.DomainModel.ItContract
{
    public class Advice : Entity
    {
        public bool IsActive { get; set; }

        public string Name { get; set; }

        public DateTime? AlarmDate { get; set; }

        public int? ReceiverId { get; set; }
        public ItContractRole Receiver { get; set; }

        public int? CarbonCopyReceiverId { get; set; }
        public ItContractRole CarbonCopyReceiver { get; set; }

        public string Subject { get; set; }

        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }
    }
}
