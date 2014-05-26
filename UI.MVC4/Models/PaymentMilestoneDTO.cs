using System;

namespace UI.MVC4.Models
{
    public class PaymentMilestoneDTO
    {
        public int Id { get; set; }
        public DateTime? Expected { get; set; }
        public DateTime? Approved { get; set; }
        public int ItContractId { get; set; }
    }
}