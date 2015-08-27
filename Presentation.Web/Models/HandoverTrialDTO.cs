using System;

namespace Presentation.Web.Models
{
    public class HandoverTrialDTO
    {
        public int Id { get; set; }
        public DateTime? Expected { get; set; }
        public DateTime? Approved { get; set; }
        public int ItContractId { get; set; }
        public int? HandoverTrialTypeId { get; set; }
    }
}
