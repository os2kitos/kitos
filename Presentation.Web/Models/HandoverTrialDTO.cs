using System;
using Newtonsoft.Json;
using Presentation.Web.Filters;

namespace Presentation.Web.Models
{
    public class HandoverTrialDTO
    {
        public int Id { get; set; }
        [JsonConverter(typeof(Rfc3339FullDateConverter))]
        public DateTime? Expected { get; set; }
        [JsonConverter(typeof(Rfc3339FullDateConverter))]
        public DateTime? Approved { get; set; }
        public int ItContractId { get; set; }
        public int? HandoverTrialTypeId { get; set; }
    }
}
