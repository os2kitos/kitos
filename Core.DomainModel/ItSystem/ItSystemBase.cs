using System;

namespace Core.DomainModel.ItSystem
{
    public abstract class ItSystemBase : Entity, IContextAware, ISystemModule, IOwnedByOrganization, IHasAccessModifier, IHasName
    {
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
      
        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            return OrganizationId == organizationId;
        }
    }
}
