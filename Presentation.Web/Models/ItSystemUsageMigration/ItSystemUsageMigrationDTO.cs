using System.Collections.Generic;

namespace Presentation.Web.Models.ItSystemUsageMigration
{
    public class ItSystemUsageMigrationDTO
    {
        public NamedEntityDTO TargetUsage { get; set; }
        public NamedEntityDTO FromSystem { get; set; }
        public NamedEntityDTO ToSystem { get; set; }
        public IEnumerable<NamedEntityDTO> AffectedItProjects { get; set; }
        public IEnumerable<ItSystemUsageContractMigrationDTO> AffectedContracts { get; set; }

    }
}