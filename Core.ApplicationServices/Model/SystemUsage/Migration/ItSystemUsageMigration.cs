using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Core.ApplicationServices.Model.SystemUsage.Migration
{
    public class ItSystemUsageMigration
    {
        public DomainModel.ItSystemUsage.ItSystemUsage ItSystemUsage { get; }
        public ItSystem FromItSystem { get; }
        public ItSystem ToItSystem { get; }
        public IReadOnlyList<ItProject> AffectedProjects { get; }
        public IReadOnlyList<ItContractMigration> AffectedContracts { get; }

        public ItSystemUsageMigration(
            DomainModel.ItSystemUsage.ItSystemUsage itSystemUsage,
            ItSystem fromItSystem,
            ItSystem itSystem,
            IEnumerable<ItProject> affectedProjects, 
            IEnumerable<ItContractMigration> affectedContracts)
        {
            ItSystemUsage = itSystemUsage;
            FromItSystem = fromItSystem;
            ToItSystem = itSystem;
            AffectedProjects = affectedProjects.ToList().AsReadOnly();
            AffectedContracts = affectedContracts.ToList().AsReadOnly();
        }
    }
}
