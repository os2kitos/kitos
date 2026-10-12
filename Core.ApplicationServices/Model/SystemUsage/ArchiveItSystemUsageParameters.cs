using Core.ApplicationServices.Model.SystemUsage.Write;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Model.SystemUsage
{
    public class ArchiveItSystemUsageParameters
    {
        public required DateTime ArchivingDate { get; set; }
        public DateTime? TakenIntoUsageDate { get; set; }

        public required string? ReferenceName { get; set; }
        public required string Note { get; set; }
        public IEnumerable<NamedLink> ArchiveReferences { get; set; } = new List<NamedLink>();
    }
}
