using System;
using Newtonsoft.Json;
using UI.MVC4.Filters;

namespace UI.MVC4.Models
{
    public class ActivityDTO
    {
        public int Id { get; set; }

        /// <summary>
        /// Human readable ID ("brugervendt noegle" in OIO)
        /// </summary>
        public string HumanReadableId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }
        public string Note { get; set; }
        
        public int TimeEstimate { get; set; }
        
        /// <summary>
        /// Procentage of activity status
        /// </summary>
        public int StatusProcentage { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? StartDate { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? EndDate { get; set; }

        public int? AssociatedUserId { get; set; }
        public UserDTO AssociatedUser { get; set; }

        public int ObjectOwnerId { get; set; }
        public UserDTO ObjectOwner { get; set; }

        public int? AssociatedActivityId { get; set; }

        public int? TaskForProjectId { get; set; }
    }
}