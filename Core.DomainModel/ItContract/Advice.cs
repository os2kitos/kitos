using System;

namespace Core.DomainModel.ItContract
{
    public class Advice : IEntity<int>
    {
        public int Id { get; set; }

        public bool IsActive { get; set; }

        public DateTime AlarmDate { get; set; }

        public int? ToId { get; set; }
        public ItContractRole To { get; set; }

        public int? CcId { get; set; }
        public ItContractRole Cc { get; set; }

        public string Subject { get; set; }

        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }
    }
}
