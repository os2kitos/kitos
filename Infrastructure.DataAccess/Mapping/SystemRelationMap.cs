using Core.DomainModel.ItSystemUsage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class SystemRelationMap : EntityMap<SystemRelation>
    {
        public override void Configure(EntityTypeBuilder<SystemRelation> builder)
        {
            base.Configure(builder);
            base.Configure(builder);

            builder.HasOne(t => t.UsageFrequency)
                .WithMany(d => d.References)
                .HasForeignKey(x => x.UsageFrequencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssociatedContract)
                .WithMany(x => x.AssociatedSystemRelations)
                .HasForeignKey(x => x.AssociatedContractId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RelationInterface)
                .WithMany(x => x.AssociatedSystemRelations)
                .HasForeignKey(x => x.RelationInterfaceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ToSystemUsage)
                .WithMany(x => x.UsedByRelations)
                .HasForeignKey(x => x.ToSystemUsageId)
                .IsRequired()
                .OnDelete(DeleteBehavior.ClientCascade);

            builder.HasOne(x => x.FromSystemUsage)
                .WithMany(x => x.UsageRelations)
                .HasForeignKey(x => x.FromSystemUsageId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
