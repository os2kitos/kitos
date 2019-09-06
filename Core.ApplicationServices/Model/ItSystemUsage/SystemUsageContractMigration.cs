using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Model.ItSystemUsage
{
    public class SystemUsageContractMigration
    {
        public ItContract Contract { get; }
        public IReadOnlyList<ItInterfaceUsage> AffectedUsages { get; }
        public IReadOnlyList<ItInterfaceExhibitUsage> ExhibitUsagesToBeDeleted { get; }
    }
}
