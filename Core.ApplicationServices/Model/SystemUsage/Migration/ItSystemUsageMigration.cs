using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Model.SystemUsage.Migration
{
    public class ItSystemUsageMigration
    {
        public DomainModel.ItSystemUsage.ItSystemUsage SystemUsage { get; }
        public ItSystem FromItSystem { get; }
        public ItSystem ToItSystem { get; }
        public IReadOnlyList<ItProject> AffectedProjects { get; }
        public IReadOnlyList<ItContract> AffectedContracts { get; }
        public IReadOnlyList<SystemRelation> AffectedRelations { get; }

        public ItSystemUsageMigration(
            DomainModel.ItSystemUsage.ItSystemUsage systemUsage,
            ItSystem fromItSystem,
            ItSystem toItSystem,
            IEnumerable<ItProject> affectedProjects, 
            IEnumerable<ItContract> affectedContracts,
            IEnumerable<SystemRelation> affectedRelations)
        {
            SystemUsage = systemUsage;
            FromItSystem = fromItSystem;
            ToItSystem = toItSystem;
            AffectedProjects = affectedProjects.ToList().AsReadOnly();
            AffectedContracts = affectedContracts.ToList().AsReadOnly();
            AffectedRelations = affectedRelations.ToList().AsReadOnly();
        }
    }
}
