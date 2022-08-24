using System;
using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project handover data.
    /// </summary>
    public class Handover : Entity, IProjectModule, ISupportsUserSpecificAccessControl
    {
        public Handover()
        {
            this.Participants = new List<User>();
        }

        public string Description { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the associated it project.
        /// </summary>
        /// <value>
        /// It project.
        /// </value>
        public virtual ItProject ItProject { get; set; }
        /// <summary>
        /// Gets or sets the users that participated in the handover.
        /// </summary>
        /// <value>
        /// The participants.
        /// </value>
        public virtual ICollection<User> Participants { get; set; }

        public bool HasUserWriteAccess(User user)
        {
            return ItProject != null && ItProject.HasUserWriteAccess(user);
        }
    }
}
