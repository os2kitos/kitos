using System;
using Newtonsoft.Json;
using UI.MVC4.Filters;

namespace UI.MVC4.Models
{
    public class MilestoneDTO : ItProjectStatusDTO
    {
        /// <summary>
        /// Which date, the state should be reached
        /// </summary>
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Trafic light status for the state
        /// </summary>
        public int Status { get; set; }
    }
}