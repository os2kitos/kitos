using System;

namespace Core.DomainModel.ItProject
{
    public class ItProjectStatusUpdate: Entity, IContextAware, IOwnedByOrganization, IProjectModule
    {
        public ItProjectStatusUpdate()
        {
            // instance creation time
            Created = DateTime.UtcNow;
        }
        public int? AssociatedItProjectId { get; set; }
        /// <summary>
        /// Gets or sets the associated it project.
        /// </summary>
        /// <value>
        /// The associated it project.
        /// </value>
        public virtual ItProject AssociatedItProject { get; set; }

        public bool IsCombined { get; set; }
        public string Note { get; set; }
        public TrafficLight TimeStatus { get; set; }
        public TrafficLight QualityStatus { get; set; }
        public TrafficLight ResourcesStatus { get; set; }
        public TrafficLight CombinedStatus { get; set; }
        public DateTime Created { get; set; }
        public int OrganizationId { get; set; }
        public virtual Organization.Organization Organization { get; set; }
        public bool IsFinal { get; set; }
        public bool IsInContext(int organizationId)
        {
            return organizationId == this.OrganizationId;
        }
    }
}
