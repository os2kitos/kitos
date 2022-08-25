namespace Core.DomainModel.ItProject
{
    public class Stakeholder : Entity, IProjectModule, ISupportsUserSpecificAccessControl
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

        public bool HasUserWriteAccess(User user)
        {
            return ItProject != null && ItProject.HasUserWriteAccess(user);
        }
    }
}
