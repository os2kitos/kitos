using System;
using Newtonsoft.Json;
using Presentation.Web.Filters;

namespace Presentation.Web.Models
{
    public class AdviceDTO
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        [JsonConverter(typeof(Rfc3339FullDateConverter))]
        public DateTime? AlarmDate { get; set; }
        public int? ReceiverId { get; set; }
        public int? CarbonCopyReceiverId { get; set; }
        public string Subject { get; set; }
        public int ItContractId { get; set; }
        [JsonConverter(typeof(Rfc3339FullDateConverter))]
        public DateTime? SentDate { get; set; }
    }
}
