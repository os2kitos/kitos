using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class SystemRelationWriteRequestDTO
    {
        /// <summary>
        /// Identifies the system usage the relation points to
        /// </summary>
        [Required]
        [NonEmptyGuid]
        public Guid ToSystemUsageUuid { get; set; }
        /// <summary>
        /// The relation uses the interface
        /// The interface must be exposed by the system identified by ToSystemUsageUuid
        /// </summary>
        [NonEmptyGuid]
        public Guid? RelationInterfaceUuid { get; set; }
        /// <summary>
        /// The contract association of the system relation
        /// The contract must be defined in the same organization as the it-system usages.
        /// </summary>
        [NonEmptyGuid]
        public Guid? AssociatedContractUuid { get; set; }
        /// <summary>
        /// Frequency of the relation
        /// If part of a new relation or a change, the option must be enabled in the organization context.
        /// </summary>
        [NonEmptyGuid]
        public Guid? RelationFrequencyUuid { get; set; }

        public string Description { get; set; }
        public string UrlReference { get; set; }
    }
}