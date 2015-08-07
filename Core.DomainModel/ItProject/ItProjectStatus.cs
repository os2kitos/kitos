namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// Base class of Milestone and Assignment.
    /// Called status for lack of a better word.
    /// </summary>
    public abstract class ItProjectStatus : Entity, IContextAware
    {
        /// <summary>
        /// Human readable ID ("brugervendt noegle" in OIO)
        /// </summary>
        public string HumanReadableId { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Description of the state
        /// </summary>
        public string Description { get; set; }
        public string Note { get; set; }

        /// <summary>
        /// Estimate for the time needed to reach this state
        /// </summary>
        public int TimeEstimate { get; set; }

        public int? AssociatedUserId { get; set; }
        /// <summary>
        /// User which is somehow associated with this state
        /// </summary>
        public virtual User AssociatedUser { get; set; }
        
        public int? AssociatedItProjectId { get; set; }
        /// <summary>
        /// Gets or sets the associated it project.
        /// </summary>
        /// <value>
        /// The associated it project.
        /// </value>
        public virtual ItProject AssociatedItProject { get; set; }

        /// <summary>
        /// Gets or sets the associated phase identifier.
        /// This id relates to a phase in the <see cref="AssociatedItProject"/>.
        /// </summary>
        /// <value>
        /// The associated phase identifier.
        /// </value>
        public int? AssociatedPhaseId { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (AssociatedItProject != null && AssociatedItProject.HasUserWriteAccess(user)) 
                return true;

            return base.HasUserWriteAccess(user);
        }

        public bool IsInContext(int organizationId)
        {
            // delegate to it project
            if (AssociatedItProject != null)
                return AssociatedItProject.IsInContext(organizationId);

            return false;
        }
    }
}
