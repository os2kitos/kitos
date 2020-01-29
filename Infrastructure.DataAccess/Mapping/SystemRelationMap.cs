using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Mapping
{
    public class SystemRelationMap : EntityMap<SystemRelation>
    {
        public SystemRelationMap()
        {
            this.HasOptional(t => t.UsageFrequency)
                .WithMany(d => d.References)
                .HasForeignKey(x => x.UsageFrequencyId)
                .WillCascadeOnDelete(false);

            this.HasOptional(x => x.AssociatedContract)
                .WithMany(x => x.AssociatedSystemRelations)
                .HasForeignKey(x => x.AssociatedContractId)
                .WillCascadeOnDelete(false);

            this.HasOptional(x => x.RelationInterface)
                .WithMany(x => x.AssociatedSystemRelations)
                .HasForeignKey(x => x.RelationInterfaceId);

            this.HasRequired(x => x.RelationTarget)
                .WithMany(x => x.UsedByRelations)
                .HasForeignKey(x => x.RelationTargetId)
                .WillCascadeOnDelete(false);

            this.HasRequired(x => x.RelationSource)
                .WithMany(x => x.UsageRelations)
                .HasForeignKey(x => x.RelationSourceId)
                .WillCascadeOnDelete(true);

            this.Property(x => x.Reference).IsOptional();
            this.Property(x => x.Description).IsOptional();
        }
    }
}
