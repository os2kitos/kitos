using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.External.V2.Types.SystemUsage;

namespace Presentation.Web.Models.External.V2.Request.SystemUsage
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
        public Guid? ArchiveTypeUuid { get; set; }
        /// <summary>
        /// Identifies the physical location for archive receiving the data
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        [NonEmptyGuid]
        public Guid? ArchiveLocationUuid { get; set; }
        /// <summary>
        /// Identifies the physical location for the archiving test
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        [NonEmptyGuid]
        public Guid? ArchiveTestLocationUuid { get; set; }
        [NonEmptyGuid]
        public Guid? ArchiveSupplierOrganizationUuid { get; set; }
        /// <summary>
        /// Determines if any archiving has occurred from this system
        /// </summary>
        public bool? ArchiveActive { get; set; }
        /// <summary>
        /// Archiving notes
        /// </summary>
        public string ArchiveNotes { get; set; }
        /// <summary>
        /// Determines the frequency of the archiving activity
        /// </summary>
        public int? ArchiveFrequencyInMonths { get; set; }

        public bool? ArchiveDocumentBearing { get; set; }
        public IEnumerable<JournalPeriodDTO> ArchiveJournalPeriods { get; set; }
    }
}