using System;
using Newtonsoft.Json;
using Presentation.Web.Filters;

namespace Presentation.Web.Models
{
    public class AssignmentDTO : ItProjectStatusDTO
    {
        /// <summary>
        /// Procentage of activity status
        /// </summary>
        public int StatusProcentage { get; set; }

        [JsonConverter(typeof(Rfc3339FullDateConverter))]
        public DateTime? StartDate { get; set; }
        [JsonConverter(typeof(Rfc3339FullDateConverter))]
        public DateTime? EndDate { get; set; }
    }
}
