using System;

namespace Presentation.Web.Models.API.V1
{
    public class PaymentMilestoneDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? Expected { get; set; }
        public DateTime? Approved { get; set; }
        public int ItContractId { get; set; }
    }
}
