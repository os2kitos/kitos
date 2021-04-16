using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
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

            Property(x => x.ResponsibleOrganizationUnitName)
                .HasMaxLength(OrganizationUnit.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationName", 0);

            Property(x => x.ItSystemBusinessTypeId)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeId", 0);

            Property(x => x.ItSystemBusinessTypeName)
                .HasMaxLength(OptionEntity<ItSystem>.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeName", 0);

            Property(x => x.ItSystemRightsHolderId)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemBelongsToId", 0);

            Property(x => x.ItSystemRightsHolderName)
                .HasMaxLength(Organization.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemBelongsToName", 0);

            Property(x => x.LocalOverviewReferenceTitle)
                .HasMaxLength(ItSystemUsageOverviewReadModel.MaxReferenceTitleLenght)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_LocalOverviewReferenceTitle", 0);


            //No index bc we don't know how long it might be
            Property(x => x.ItSystemKLEIdsAsCsv).IsOptional();
            Property(x => x.ItSystemKLENamesAsCsv).IsOptional();

            Property(x => x.LocalOverviewReferenceTitle).IsOptional();
            Property(x => x.LocalOverviewReferenceUrl).IsOptional();



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
