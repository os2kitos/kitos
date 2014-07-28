using System;
using Newtonsoft.Json;
using UI.MVC4.Filters;

namespace UI.MVC4.Models
{
    public class ItProjectPhaseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? StartDate { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? EndDate { get; set; }
        public int? AssociatedItProjectId { get; set; } 
    }
}
