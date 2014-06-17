using System;

namespace UI.MVC4.Models
{
    public class StateDTO
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
        /// Which date, the state should be reached
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Trafic light status for the state
        /// </summary>
        public int Status { get; set; }

        public int? AssociatedUserId { get; set; }
        public virtual UserDTO AssociatedUser { get; set; }

        public int? AssociatedActivityId { get; set; }

        public int ObjectOwnerId { get; set; }
        public virtual UserDTO ObjectOwner { get; set; }

        public int? MilestoneForProjectId { get; set; }
    }
}