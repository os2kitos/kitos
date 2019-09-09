using System.Collections.Generic;

namespace Presentation.Web.Models.ItSystemUsageMigration
{
    public class ItSystemUsageContractMigrationDTO
    {
        public NamedEntityDTO Contract { get; set; }
        public IEnumerable<NamedEntityDTO> AffectedInterfaceUsages { get; set; }
        public IEnumerable<NamedEntityDTO> InterfaceExhibitUsagesToBeDeleted { get; set; }

    }
}