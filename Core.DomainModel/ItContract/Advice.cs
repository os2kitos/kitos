using System;

namespace Core.DomainModel.ItContract
{
    public class Advice : IEntity<int>
    {
        public int Id { get; set; }

        public bool IsActive { get; set; }

        public DateTime AlarmDate { get; set; }

        public int? ReceiverId { get; set; }
        public ItContractRole Receiver { get; set; }

        public int? CarbonCopyReceiverId { get; set; }
        public ItContractRole CarbonCopyReceiver { get; set; }

        public string Subject { get; set; }

        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }
    }
}
