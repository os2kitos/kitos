using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Mapping
{
    public class SystemRelationMap : EntityMap<SystemRelation>
    {
        public SystemRelationMap()
        {
            this.HasOptional(t => t.UsageFrequency)
                .WithMany(d => d.References);

            this.HasOptional(x => x.AssociatedContract);
            this.HasOptional(x => x.RelationInterface);

            this.HasRequired(x => x.RelationTarget)
                .WithMany(x => x.UsedByRelations)
                .HasForeignKey(x => x.RelationTargetId)
                .WillCascadeOnDelete(false);

            this.HasRequired(x => x.RelationSource)
                .WithMany(x => x.UsageRelations)
                .HasForeignKey(x => x.RelationSourceId)
                .WillCascadeOnDelete(true);

            this.HasRequired(x => x.Reference);
            this.Property(x => x.Description).IsOptional();
        }
    }
}
