using System;
using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project goal status.
    /// </summary>
    public class GoalStatus : Entity, IContextAware
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

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (ItProject != null && ItProject.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }

        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            if (ItProject != null)
                return ItProject.IsInContext(organizationId);

            return false;
        }
    }
}
