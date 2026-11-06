using System;

namespace Core.DomainModel.ItContract
{
    public class ItContractInvoiceDate
    {
        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }
        public int Period { get; set; }
        public DateTime Date { get; set; }
    }
}
