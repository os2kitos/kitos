using System;
using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project goal status.
    /// </summary>
    public class GoalStatus : Entity, IProjectModule, ISupportsUserSpecificAccessControl
    {
        public GoalStatus()
        {
            this.Goals = new List<Goal>();
        }

        public virtual ItProject ItProject { get; set; }

        /// <summary>
        /// Traffic-light dropdown for overall status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public TrafficLight Status { get; set; }
        /// <summary>
        /// Date-for-status-update field
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// Notes on collected status on project
        /// </summary>
        public string StatusNote { get; set; }

        public virtual ICollection<Goal> Goals { get; set; }

        public bool HasUserWriteAccess(User user)
        {
            return ItProject != null && ItProject.HasUserWriteAccess(user);
        }
    }
}
