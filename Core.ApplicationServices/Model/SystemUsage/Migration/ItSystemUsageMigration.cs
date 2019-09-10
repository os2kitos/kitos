using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Core.ApplicationServices.Model.SystemUsage.Migration
{
    public class ItSystemUsageMigration
    {
        public DomainModel.ItSystemUsage.ItSystemUsage SystemUsage { get; }
        public ItSystem FromItSystem { get; }
        public ItSystem ToItSystem { get; }
        public IReadOnlyList<ItProject> AffectedProjects { get; }
        public IReadOnlyList<ItContractMigration> AffectedContracts { get; }

        public ItSystemUsageMigration(
            DomainModel.ItSystemUsage.ItSystemUsage systemUsage,
            ItSystem fromItSystem,
            ItSystem toItSystem,
            IEnumerable<ItProject> affectedProjects, 
            IEnumerable<ItContractMigration> affectedContracts)
        {
            SystemUsage = systemUsage;
            FromItSystem = fromItSystem;
            ToItSystem = toItSystem;
            AffectedProjects = affectedProjects.ToList().AsReadOnly();
            AffectedContracts = affectedContracts.ToList().AsReadOnly();
        }
    }
}
