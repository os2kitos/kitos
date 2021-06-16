using System;

namespace Core.DomainModel.ItSystem
{
    public abstract class ItSystemBase : Entity, ISystemModule, IOwnedByOrganization, IHasAccessModifier, IHasName, IHasUuid
    {
        protected ItSystemBase()
        {
            Uuid = Guid.NewGuid();
        }

        public string Name { get; set; }
        public Guid Uuid { get; set; }
        public string Description { get; set; }
        
        public AccessModifier AccessModifier { get; set; }

        public int OrganizationId { get; set; }
        /// <summary>
        /// Gets or sets the organization this instance was created under.
        /// </summary>
        /// <value>
        /// The organization.
        /// </value>
        public virtual Organization.Organization Organization { get; set; }

        public DateTime? Created { get; set; }
    }
}
