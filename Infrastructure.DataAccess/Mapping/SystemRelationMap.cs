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
            this.HasRequired(x => x.RelationTarget);
            this.HasRequired(x => x.RelationSource);
            this.HasRequired(x => x.Reference);
            this.Property(x => x.Description).IsOptional();
        }
    }
}
