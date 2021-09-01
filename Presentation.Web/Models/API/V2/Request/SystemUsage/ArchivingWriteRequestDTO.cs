using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class ArchivingWriteRequestDTO
    {
        /// <summary>
        /// Archive duty for the system in the organization context. The recommendation from the archiving authority is found on the IT-System context.
        /// </summary>
        public ArchiveDutyChoice? ArchiveDuty { get; set; }
        /// <summary>
        /// Defines the archiving type associated with the it-system
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        [NonEmptyGuid]
        public Guid? TypeUuid { get; set; }
        /// <summary>
        /// Identifies the physical location for archive receiving the data
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        [NonEmptyGuid]
        public Guid? LocationUuid { get; set; }
        /// <summary>
        /// Identifies the physical location for the archiving test
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        [NonEmptyGuid]
        public Guid? TestLocationUuid { get; set; }
        [NonEmptyGuid]
        public Guid? SupplierOrganizationUuid { get; set; }
        /// <summary>
        /// Determines if any archiving has occurred from this system
        /// </summary>
        public bool? Active { get; set; }
        /// <summary>
        /// Archiving notes
        /// </summary>
        public string Notes { get; set; }
        /// <summary>
        /// Determines the frequency of the archiving activity
        /// </summary>
        public int? FrequencyInMonths { get; set; }

        public bool? DocumentBearing { get; set; }
        public IEnumerable<JournalPeriodDTO> JournalPeriods { get; set; }
    }
}