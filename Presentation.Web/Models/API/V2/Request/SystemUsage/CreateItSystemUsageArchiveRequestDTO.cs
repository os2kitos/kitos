using Presentation.Web.Models.API.V2.Types.Shared;
using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class CreateItSystemUsageArchiveRequestDTO
    {
        /// <summary>
        /// The date when the system usage was archived
        /// </summary>
        public required DateTime ArchivingDate { get; set; }

        /// <summary>
        /// The date when the system was taken into usage
        /// </summary>
        public DateTime? TakenIntoUsageDate { get; set; }

        /// <summary>
        /// A reference name for the archive
        /// </summary>
        public string? ReferenceName { get; set; }

        /// <summary>
        /// Notes about the archive
        /// </summary>
        public required string Note { get; set; }

        /// <summary>
        /// References associated with the archive
        /// </summary>
        public IEnumerable<SimpleLinkDTO> ArchiveReferences { get; set; } = new List<SimpleLinkDTO>();
    }
}
