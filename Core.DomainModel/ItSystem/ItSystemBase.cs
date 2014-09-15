using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public abstract class ItSystemBase : Entity
    {
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the user defined system identifier.
        /// </summary>
        /// <remarks>
        /// This identifier is NOT the primary key.
        /// </remarks>
        /// <value>
        /// The user defined system identifier.
        /// </value>
        public string ItSystemId { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public AccessModifier AccessModifier { get; set; }

        public int OrganizationId { get; set; }
        /// <summary>
        /// Gets or sets the organization this instance was created under.
        /// </summary>
        /// <value>
        /// The organization.
        /// </value>
        public virtual Organization Organization { get; set; }
    }
}