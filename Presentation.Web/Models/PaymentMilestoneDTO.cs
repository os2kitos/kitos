using System;
using Newtonsoft.Json;
using Presentation.Web.Filters;

namespace Presentation.Web.Models
{
    public class PaymentMilestoneDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? Expected { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? Approved { get; set; }
        public int ItContractId { get; set; }
    }
}
