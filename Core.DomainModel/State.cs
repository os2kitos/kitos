using System;

namespace Core.DomainModel
{
    public class State : Entity
    {

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
        public virtual User AssociatedUser { get; set; }

        public int? AssociatedActivityId { get; set; }

        /// <summary>
        /// The state might be a milestone for an IT project
        /// </summary>
        public virtual ItProject.ItProject MilestoneForProject { get; set; }
        public int? MilestoneForProjectId { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (MilestoneForProject != null && MilestoneForProject.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
