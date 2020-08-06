namespace Core.DomainModel.ItProject
{
    public class Stakeholder : Entity, IProjectModule
    {
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

        public string Name { get; set; }
        public string Role { get; set; }
        public string Downsides { get; set; }
        public string Benefits { get; set; }
        public int Significance { get; set; }
        public string HowToHandle { get; set; }

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (ItProject != null && ItProject.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
