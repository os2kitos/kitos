using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.External.V2.Request.SystemUsage
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
        /// </summary>
        [NonEmptyGuid]
        public Guid? UsingInterfaceUuid { get; set; }
        /// <summary>
        /// The contract association of the system relation
        /// </summary>
        [NonEmptyGuid]
        public Guid? AssociatedContractUuid { get; set; }
        /// <summary>
        /// Frequency of the relation
        /// </summary>
        [NonEmptyGuid]
        public Guid? RelationFrequencyUuid { get; set; }

        public string Description { get; set; }
        public string UrlReference { get; set; }
    }
}