using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class SystemRelationResponseDTO : IHasUuidExternal
    {
        /// <summary>
        /// UUID of the system relation registration
        /// </summary>
        public Guid Uuid { get; set; }
        /// <summary>
        /// Identifies the system usage the relation points to
        /// </summary>
        public IdentityNamePairResponseDTO ToSystemUsage { get; set; }
        /// <summary>
        /// The relation uses the interface
        /// </summary>
        public IdentityNamePairResponseDTO RelationInterface { get; set; }
        /// <summary>
        /// The contract association of the system relation
        /// </summary>
        public IdentityNamePairResponseDTO AssociatedContract { get; set; }
        /// <summary>
        /// Frequency of the relation
        /// </summary>
        public IdentityNamePairResponseDTO RelationFrequency { get; set; }

        public string Description { get; set; }
        public string UrlReference { get; set; }
        
    }
}