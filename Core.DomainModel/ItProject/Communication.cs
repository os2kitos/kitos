using System;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project communication data.
    /// </summary>
    public class Communication : Entity, IProjectModule
    {
        /// <summary>
        /// Gets or sets the target audiance.
        /// </summary>
        /// <value>
        /// The target audiance.
        /// </value>
        public string TargetAudiance { get; set; }
        /// <summary>
        /// Gets or sets the purpose.
        /// </summary>
        /// <value>
        /// The purpose.
        /// </value>
        public string Purpose { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets the media.
        /// </summary>
        /// <value>
        /// The media.
        /// </value>
        public string Media { get; set; }
        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        /// <value>
        /// The due date.
        /// </value>
        public DateTime? DueDate { get; set; }
        /// <summary>
        /// Gets or sets the responsible user identifier.
        /// </summary>
        /// <value>
        /// The responsible user identifier.
        /// </value>
        public int? ResponsibleUserId { get; set; }
        /// <summary>
        /// Gets or sets the responsible user.
        /// </summary>
        /// <value>
        /// The responsible user.
        /// </value>
        public virtual User ResponsibleUser { get; set; }
        /// <summary>
        /// Gets or sets the associated it project identifier.
        /// </summary>
        /// <value>
        /// It project identifier.
        /// </value>
        public int ItProjectId { get; set; }
        /// <summary>
        /// Gets or sets the associated it project.
        /// </summary>
        /// <value>
        /// It project.
        /// </value>
        public virtual ItProject ItProject { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access; otherwise, <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (ItProject != null && ItProject.HasUserWriteAccess(user)) 
                return true;
            return base.HasUserWriteAccess(user);
        }
    }
}
