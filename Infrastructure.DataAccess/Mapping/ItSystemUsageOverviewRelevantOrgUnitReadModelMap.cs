using Core.DomainModel.ItSystemUsage.Read;
using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewRelevantOrgUnitReadModelMap: EntityTypeConfiguration<ItSystemUsageOverviewRelevantOrgUnitReadModel>
    {
        public ItSystemUsageOverviewRelevantOrgUnitReadModelMap()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Parent)
                .WithMany(x => x.RelevantOrganizationUnits)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(true);
            Property(x => x.OrganizationUnitName)
                .IsRequired()
                .HasMaxLength(OrganizationUnit.MaxNameLength)
                .HasIndexAnnotation("IX_Name");
            Property(x => x.OrganizationUnitId)
                .IsRequired()
                .HasIndexAnnotation("IX_OrgUnitId");

            Property(x => x.OrganizationUnitUuid)
                .IsRequired()
                .HasIndexAnnotation("IX_OrgUnitUuid");
        }
    }
}
