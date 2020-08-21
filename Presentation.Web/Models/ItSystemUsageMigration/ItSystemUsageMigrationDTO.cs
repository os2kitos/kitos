using System.Collections.Generic;

namespace Presentation.Web.Models.ItSystemUsageMigration
{
    public class ItSystemUsageMigrationDTO
    {
        public NamedEntityWithEnabledStatusDTO TargetUsage { get; set; }
        public NamedEntityWithEnabledStatusDTO FromSystem { get; set; }
        public NamedEntityWithEnabledStatusDTO ToSystem { get; set; }
        public IEnumerable<NamedEntityDTO> AffectedItProjects { get; set; }
        public IEnumerable<NamedEntityDTO> AffectedContracts { get; set; }
        public IEnumerable<RelationMigrationDTO> AffectedRelations { get; set; }

    }
}