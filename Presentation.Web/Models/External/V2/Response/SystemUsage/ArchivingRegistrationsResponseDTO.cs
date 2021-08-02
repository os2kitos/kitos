using System.Collections.Generic;
using Presentation.Web.Models.External.V2.Response.Organization;
using Presentation.Web.Models.External.V2.Types.Shared;

namespace Presentation.Web.Models.External.V2.Response.SystemUsage
{
    public class ArchivingRegistrationsResponseDTO
    {
        /// <summary>
        /// Archive duty for the system in the organization context. The recommendation from the archiving authority is found on the IT-System context.
        /// </summary>
        public ArchiveDutyChoice? ArchiveDuty { get; set; }
        /// <summary>
        /// Defines the archiving type associated with the it-system
        /// </summary>
        public IdentityNamePairResponseDTO Type { get; set; }
        /// <summary>
        /// Identifies the physical location for archive receiving the data
        /// </summary>
        public IdentityNamePairResponseDTO Location { get; set; }
        /// <summary>
        /// Identifies the physical location for the archiving test
        /// </summary>
        public IdentityNamePairResponseDTO TestLocation { get; set; }

        public ShallowOrganizationResponseDTO Supplier { get; set; }
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
        public IEnumerable<JournalPeriodResponseDTO> JournalPeriods { get; set; }

    }
}