namespace Core.DomainModel.ItProject
{
    public class Risk : Entity, IProjectModule, ISupportsUserSpecificAccessControl
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
        public string Action { get; set; }

        public int Probability { get; set; }
        public int Consequence { get; set; }

        public int? ResponsibleUserId { get; set; }
        public virtual User ResponsibleUser { get; set; }

        public bool HasUserWriteAccess(User user)
        {
            return ItProject != null && ItProject.HasUserWriteAccess(user);
        }
    }
}
