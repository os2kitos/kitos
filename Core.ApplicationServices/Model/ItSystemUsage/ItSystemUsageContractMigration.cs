using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Model.ItSystemUsage
{
    public class ItSystemUsageContractMigration
    {
        public ItContract Contract { get; }
        public IReadOnlyList<ItInterfaceUsage> AffectedInterfaceUsages { get; }
        public IReadOnlyList<ItInterfaceExhibitUsage> ExhibitUsagesToBeDeleted { get; }

        public ItSystemUsageContractMigration(
            ItContract contract,
            IEnumerable<ItInterfaceUsage> affectedUsages,
            IEnumerable<ItInterfaceExhibitUsage> exhibitUsagesToBeDeleted)
        {
            Contract = contract;
            AffectedInterfaceUsages = affectedUsages.ToList().AsReadOnly();
            ExhibitUsagesToBeDeleted = exhibitUsagesToBeDeleted.ToList().AsReadOnly();
        }
    }
}
