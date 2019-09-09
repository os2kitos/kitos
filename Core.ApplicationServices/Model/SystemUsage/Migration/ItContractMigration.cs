using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Model.SystemUsage.Migration
{
    public class ItContractMigration
    {
        public ItContract Contract { get; }
        public bool SystemAssociatedInContract { get; }
        public IReadOnlyList<ItInterfaceUsage> AffectedInterfaceUsages { get; }
        public IReadOnlyList<ItInterfaceExhibitUsage> ExhibitUsagesToBeDeleted { get; }

        public ItContractMigration(
            ItContract contract,
            bool systemAssociatedInContract,
            IEnumerable<ItInterfaceUsage> affectedUsages,
            IEnumerable<ItInterfaceExhibitUsage> exhibitUsagesToBeDeleted)
        {
            Contract = contract;
            SystemAssociatedInContract = systemAssociatedInContract;
            AffectedInterfaceUsages = affectedUsages.ToList().AsReadOnly();
            ExhibitUsagesToBeDeleted = exhibitUsagesToBeDeleted.ToList().AsReadOnly();
        }
    }
}
