using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemDataWorkerRelationMap: EntityMap<ItSystemDataWorkerRelation>
    {
        public ItSystemDataWorkerRelationMap()
        {
             this.HasRequired(r => r.ItSystem)
                 .WithMany(s => s.AssociatedDataWorkers)
                 .WillCascadeOnDelete(true);

            this.HasRequired(r => r.DataWorker);
        }
    }
}
