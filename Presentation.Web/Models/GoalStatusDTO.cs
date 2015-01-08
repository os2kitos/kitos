using System;
using System.Collections.Generic;
using Core.DomainModel;
using Newtonsoft.Json;
using Presentation.Web.Filters;

namespace Presentation.Web.Models
{
    public class GoalStatusDTO
    {
        public int Id { get; set; }

        public int ItProjectId { get; set; }

        /// <summary>
        /// Traffic-light dropdown for overall status
        /// </summary>
        public TrafficLight Status { get; set; }
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