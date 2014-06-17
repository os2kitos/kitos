using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UI.MVC4.Filters;

namespace UI.MVC4.Models
{
    public class GoalStatusDTO
    {
        public int Id { get; set; }

        public int ItProjectId { get; set; }

        /// <summary>
        /// Traffic-light dropdown for overall status
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// Date-for-status-update field
        /// </summary>
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// Notes on collected status on project    
        /// </summary>
        public string StatusNote { get; set; }


        public IEnumerable<GoalDTO> Goals { get; set; }
    }
}