using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public class Activity : Entity
    {
        public Activity()
        {
            this.AssociatedActivities = new List<Activity>();
            this.AssociatedStates = new List<State>();
        }

        /// <summary>
        /// Human readable ID ("brugervendt noegle" in OIO)
        /// </summary>
        public string HumanReadableId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }
        public string Note { get; set; }

        public int TimeEstimate { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Procentage of activity status
        /// </summary>
        public int StatusProcentage { get; set; }

        public int? AssociatedActivityId { get; set; }
        public virtual Activity AssociatedActivity { get; set; }

        /// <summary>
        /// Associated activities
        /// </summary>
        public virtual ICollection<Activity> AssociatedActivities { get; set; }

        /// <summary>
        /// Associated states (such as milestones, etc)
        /// </summary>
        public virtual ICollection<State> AssociatedStates { get; set; }

        public int? AssociatedUserId { get; set; }
        public virtual User AssociatedUser { get; set; }
        
        public virtual ItProject.ItProject Phase1ForProject { get; set; }
        public virtual ItProject.ItProject Phase2ForProject { get; set; }
        public virtual ItProject.ItProject Phase3ForProject { get; set; }
        public virtual ItProject.ItProject Phase4ForProject { get; set; }
        public virtual ItProject.ItProject Phase5ForProject { get; set; }

        /// <summary>
        /// The activity might be a task for an IT project
        /// </summary>
        public virtual ItProject.ItProject TaskForProject { get; set; }
        public int? TaskForProjectId { get; set; }


    }
}
