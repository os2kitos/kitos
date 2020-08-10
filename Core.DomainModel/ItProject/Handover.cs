using System;
using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project handover data.
    /// </summary>
    public class Handover : Entity, IProjectModule
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
    }
}
