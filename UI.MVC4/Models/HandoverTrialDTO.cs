using System;

namespace UI.MVC4.Models
{
    public class HandoverTrialDTO
    {
        public DateTime? Expected { get; set; }
        public DateTime? Approved { get; set; }
        public int ItContractId { get; set; }
        public int? HandoverTrialTypeId { get; set; }
    }
}