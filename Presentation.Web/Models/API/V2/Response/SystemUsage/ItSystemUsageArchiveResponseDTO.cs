using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class ItSystemUsageArchiveResponseDTO
    {
        /// <summary>
        /// Unique identifier for the archive
        /// </summary>
        public required Guid Uuid { get; set; }

        /// <summary>
        /// The date when the archive was created
        /// </summary>
        public required DateTime ArchivingDate { get; set; }

        /// <summary>
        /// A reference name for the archive
        /// </summary>
        public string? ReferenceName { get; set; }

        /// <summary>
        /// Notes about the archive
        /// </summary>
        public required string Note { get; set; }

        /// <summary>
        /// Unique identifier of the IT system from the snapshot
        /// </summary>
        public required Guid ItSystemUuid { get; set; }

        /// <summary>
        /// System name from the snapshot at archive time
        /// </summary>
        public string? LegacyName { get; set; }
        /// <summary>
        /// Date system was taken into usage
        /// </summary>
        public DateTime? TakenIntoUsageDate { get; set; }

        /// <summary>
        /// Local call name from the snapshot at archive time
        /// </summary>
        public string? LocalName { get; set; }

        /// <summary>
        /// Local system id from the snapshot at archive time
        /// </summary>
        public string? LocalId { get; set; }

        /// <summary>
        /// The organization in which the archive was created
        /// </summary>
        public ShallowOrganizationResponseDTO? Organization { get; set; }

        /// <summary>
        /// References associated with the archive
        /// </summary>
        public IEnumerable<ArchiveReferenceResponseDTO> ArchiveReferences { get; set; } = new List<ArchiveReferenceResponseDTO>();

        /// <summary>
        /// Full name of the user who archived the system usage
        /// </summary>
        public required string ArchivedByFullName { get; set; }
    }
}
