using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewItContractReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewItContractReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewItContractReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.AssociatedContracts)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.ItContractId).IsRequired();
            builder.HasIndex(x => x.ItContractId).HasDatabaseName("ItContractId");

            builder.Property(x => x.ItContractUuid).IsRequired();

            builder.Property(x => x.ItContractName).IsRequired().HasMaxLength(ItContractConstraints.MaxNameLength);
            builder.HasIndex(x => x.ItContractName).HasDatabaseName("ItContractNameName");
        }
    }
}
