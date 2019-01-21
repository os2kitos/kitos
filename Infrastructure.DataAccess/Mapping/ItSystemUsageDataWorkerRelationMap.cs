using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageDataWorkerRelationMap: EntityMap<ItSystemUsageDataWorkerRelation>
    {
        public ItSystemUsageDataWorkerRelationMap()
        {
            this.HasRequired(r => r.ItSystemUsage)
                .WithMany(s => s.AssociatedDataWorkers)
                .WillCascadeOnDelete(true);

            this.HasRequired(r => r.DataWorker)
                .WithMany()
                .WillCascadeOnDelete(false);
            
        }
    }
}
