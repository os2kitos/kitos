using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Model.SystemUsage.Migration
{
    public class ItSystemUsageMigration
    {
        public ItSystemUsage SystemUsage { get; }
        public ItSystem FromItSystem { get; }
        public ItSystem ToItSystem { get; }
        public IReadOnlyList<ItContract> AffectedContracts { get; }
        public IReadOnlyList<SystemRelation> AffectedSystemRelations { get; }
        public IReadOnlyList<DataProcessingRegistration> AffectedDataProcessingRegistrations { get; }
        public ItSystemUsageMigration(
            ItSystemUsage systemUsage,
            ItSystem fromItSystem,
            ItSystem toItSystem, 
            IEnumerable<ItContract> affectedContracts,
            IEnumerable<SystemRelation> affectedRelations,
            IEnumerable<DataProcessingRegistration> affectedDataProcessingRegistrations)
        {
            SystemUsage = systemUsage;
            FromItSystem = fromItSystem;
            ToItSystem = toItSystem;
            AffectedContracts = affectedContracts.ToList().AsReadOnly();
            AffectedSystemRelations = affectedRelations.ToList().AsReadOnly();
            AffectedDataProcessingRegistrations = affectedDataProcessingRegistrations.ToList().AsReadOnly();
        }
    }
}
