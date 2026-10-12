using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewRelevantOrgUnitReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewRelevantOrgUnitReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewRelevantOrgUnitReadModel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.RelevantOrganizationUnits)
                .HasForeignKey(x => x.ParentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.OrganizationUnitName).IsRequired().HasMaxLength(OrganizationUnit.MaxNameLength);
            builder.HasIndex(x => x.OrganizationUnitName).HasDatabaseName("IX_Name");

            builder.Property(x => x.OrganizationUnitId).IsRequired();
            builder.HasIndex(x => x.OrganizationUnitId).HasDatabaseName("IX_OrgUnitId");

            builder.Property(x => x.OrganizationUnitUuid).IsRequired();
            builder.HasIndex(x => x.OrganizationUnitUuid).HasDatabaseName("IX_OrgUnitUuid");
        }
    }
}
