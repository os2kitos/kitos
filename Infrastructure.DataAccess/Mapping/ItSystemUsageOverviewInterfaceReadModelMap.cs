using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewInterfaceReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewInterfaceReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewInterfaceReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.DependsOnInterfaces)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.InterfaceId).IsRequired();
            builder.HasIndex(x => x.InterfaceId).HasDatabaseName("ItSystemUsageOverviewInterfaceReadModel_index_InterfaceId");

            builder.Property(x => x.InterfaceUuid).IsRequired();

            builder.Property(x => x.InterfaceName).IsRequired().HasMaxLength(ItInterface.MaxNameLength);
            builder.HasIndex(x => x.InterfaceName).HasDatabaseName("ItSystemUsageOverviewInterfaceReadModel_index_InterfaceName");
        }
    }
}
