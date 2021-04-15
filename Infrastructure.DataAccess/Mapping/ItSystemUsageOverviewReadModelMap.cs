using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewReadModelMap :  EntityTypeConfiguration<ItSystemUsageOverviewReadModel>
    {
        public ItSystemUsageOverviewReadModelMap()
        {
            Property(x => x.Name)
                .HasMaxLength(ItSystem.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_Name", 0);

            Property(x => x.ItSystemDisabled)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemDisabled", 0);

            Property(x => x.Version)
                .HasMaxLength(ItSystemUsage.DefaultMaxLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_Version", 0);

            Property(x => x.LocalCallName)
                .HasMaxLength(ItSystemUsage.DefaultMaxLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_LocalCallName", 0);

            Property(x => x.LocalSystemId)
                .HasMaxLength(ItSystemUsage.DefaultMaxLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_LocalSystemId", 0);

            Property(x => x.ItSystemUuid)
                .HasMaxLength(50)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemUuid", 0);

            Property(x => x.ParentItSystemName)
                .HasMaxLength(ItSystem.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemParentName", 0);

            Property(x => x.ResponsibleOrganizationUnitId)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationId", 0);

            HasRequired(t => t.Organization)
                .WithMany(t => t.ItSystemUsageOverviewReadModels)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.SourceEntity)
                .WithMany(x => x.OverviewReadModels)
                .HasForeignKey(x => x.SourceEntityId)
                .WillCascadeOnDelete(false);
        }
    }
}
