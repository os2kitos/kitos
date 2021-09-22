using System;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractPaymentMilestone
    {
        public string Title { get; set; }
        public DateTime? Expected { get; set; }
        public DateTime? Approved { get; set; }
    }
}