using System;
using Newtonsoft.Json;
using UI.MVC4.Filters;

namespace UI.MVC4.Models
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