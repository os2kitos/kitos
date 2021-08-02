using System;
using System.Collections.Generic;
using Presentation.Web.Models.External.V2.Types.Shared;
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
        /// </summary>
        public Guid? ArchiveTypeUuid { get; set; }
        /// <summary>
        /// Identifies the physical location for archive receiving the data
        /// </summary>
        public Guid? ArchiveLocationUuid { get; set; }
        /// <summary>
        /// Identifies the physical location for the archiving test
        /// </summary>
        public Guid? ArchiveTestLocationUuid { get; set; }
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