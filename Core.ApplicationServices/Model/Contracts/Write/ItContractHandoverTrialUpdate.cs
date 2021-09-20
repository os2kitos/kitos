using System;

namespace Core.ApplicationServices.Model.Contracts.Write
{
    public class ItContractHandoverTrialUpdate
    {
        public Guid HandoverTrialTypeUuid { get; set; }
        public DateTime? ExpectedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
