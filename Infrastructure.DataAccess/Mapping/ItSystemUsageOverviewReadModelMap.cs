using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewReadModelMap : IEntityTypeConfiguration<ItSystemUsageOverviewReadModel>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOverviewReadModel> builder)
        {
            builder.Property(x => x.SystemName).HasMaxLength(ItSystem.MaxNameLength).IsRequired();
            builder.HasIndex(x => x.SystemName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_Name");

            builder.Property(x => x.ItSystemDisabled).IsRequired();
            builder.HasIndex(x => x.ItSystemDisabled).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemDisabled");

            builder.Property(x => x.Version).HasMaxLength(ItSystemUsage.DefaultMaxLength);
            builder.HasIndex(x => x.Version).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_Version");

            builder.Property(x => x.LocalCallName).HasMaxLength(ItSystemUsage.DefaultMaxLength);
            builder.HasIndex(x => x.LocalCallName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_LocalCallName");

            builder.Property(x => x.LocalSystemId).HasMaxLength(ItSystemUsage.LongProperyMaxLength);
            builder.HasIndex(x => x.LocalSystemId).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_LocalSystemId");

            builder.Property(x => x.ItSystemUuid).HasMaxLength(50);
            builder.HasIndex(x => x.ItSystemUuid).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemUuid");

            builder.HasIndex(x => x.ParentItSystemUsageUuid).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ParentItSystemUsageUuid");

            builder.Property(x => x.ParentItSystemName).HasMaxLength(ItSystem.MaxNameLength);
            builder.HasIndex(x => x.ParentItSystemName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemParentName");

            builder.HasIndex(x => x.ResponsibleOrganizationUnitId).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationId");
            builder.HasIndex(x => x.ResponsibleOrganizationUnitUuid).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationUuid");

            builder.Property(x => x.ResponsibleOrganizationUnitName).HasMaxLength(OrganizationUnit.MaxNameLength);
            builder.HasIndex(x => x.ResponsibleOrganizationUnitName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationName");

            builder.HasIndex(x => x.ItSystemBusinessTypeUuid).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeUuid");
            builder.HasIndex(x => x.ItSystemBusinessTypeId).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeId");

            builder.Property(x => x.ItSystemBusinessTypeName).HasMaxLength(OptionEntity<ItSystem>.MaxNameLength);
            builder.HasIndex(x => x.ItSystemBusinessTypeName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeName");

            builder.HasIndex(x => x.ItSystemRightsHolderId).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemBelongsToId");

            builder.Property(x => x.ItSystemRightsHolderName).HasMaxLength(Organization.MaxNameLength);
            builder.HasIndex(x => x.ItSystemRightsHolderName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemBelongsToName");

            builder.HasIndex(x => x.ItSystemCategoriesUuid).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesUuid");
            builder.HasIndex(x => x.ItSystemCategoriesId).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesId");

            builder.Property(x => x.ItSystemCategoriesName).HasMaxLength(Organization.MaxNameLength);
            builder.HasIndex(x => x.ItSystemCategoriesName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesName");

            builder.Property(x => x.LocalReferenceTitle).HasMaxLength(ItSystemUsageOverviewReadModel.MaxReferenceTitleLength);
            builder.HasIndex(x => x.LocalReferenceTitle).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_LocalReferenceTitle");

            builder.HasIndex(x => x.ObjectOwnerId).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ObjectOwnerId");

            builder.Property(x => x.ObjectOwnerName).HasMaxLength(UserConstraints.MaxNameLength);
            builder.HasIndex(x => x.ObjectOwnerName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ObjectOwnerName");

            builder.HasIndex(x => x.LastChangedById).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_LastChangedById");

            builder.Property(x => x.LastChangedByName).HasMaxLength(UserConstraints.MaxNameLength);
            builder.HasIndex(x => x.LastChangedByName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_LastChangedByName");

            builder.HasIndex(x => x.MainContractId).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_MainContractId");
            builder.HasIndex(x => x.MainContractSupplierId).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_MainContractSupplierId");

            builder.Property(x => x.MainContractSupplierName).HasMaxLength(Organization.MaxNameLength);
            builder.HasIndex(x => x.MainContractSupplierName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_MainContractSupplierName");

            builder.HasIndex(x => x.ArchiveDuty).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ArchiveDuty");
            builder.HasIndex(x => x.CatalogArchiveDuty).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_CatalogArchiveDuty");
            builder.HasIndex(x => x.IsHoldingDocument).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_IsHoldingDocument");

            builder.Property(x => x.RiskSupervisionDocumentationName).HasMaxLength(ItSystemUsage.LinkNameMaxLength);
            builder.HasIndex(x => x.RiskSupervisionDocumentationName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_RiskSupervisionDocumentationName");

            builder.Property(x => x.LinkToDirectoryName).HasMaxLength(ItSystemUsage.LinkNameMaxLength);
            builder.HasIndex(x => x.LinkToDirectoryName).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_LinkToDirectoryName");

            builder.Property(x => x.GeneralPurpose).HasMaxLength(ItSystemUsage.LongProperyMaxLength);
            builder.HasIndex(x => x.GeneralPurpose).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_GeneralPurpose");

            builder.Property(x => x.ProcessingPurpose).HasMaxLength(ItSystemUsage.LongProperyMaxLength);
            builder.HasIndex(x => x.ProcessingPurpose).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ProcessingPurpose");

            builder.HasIndex(x => x.HostedAt).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_HostedAt");
            builder.HasIndex(x => x.UserCount).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_UserCount");
            builder.HasIndex(x => x.DPIAConducted).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_DPIAConducted");
            builder.HasIndex(x => x.IsBusinessCritical).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_IsBusinessCritical");
            builder.HasIndex(x => x.ActiveAccordingToValidityPeriod).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ActiveAccordingToValidityPeriod");
            builder.HasIndex(x => x.ActiveAccordingToLifeCycle).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_ActiveAccordingToLifeCycle");
            builder.HasIndex(x => x.LifeCycleStatus).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_LifeCycleStatus");
            builder.HasIndex(x => x.Concluded).HasDatabaseName("IX_Concluded");
            builder.HasIndex(x => x.ExpirationDate).HasDatabaseName("IX_ExpirationDate");
            builder.HasIndex(x => x.SystemUsageCriticalityLevelUuid).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_SystemUsageCriticalityLevelUuid");
            builder.HasIndex(x => x.SystemActive).HasDatabaseName("ItSystemUsageOverviewReadModel_Index_SystemActive");
            builder.HasIndex(x => x.RiskAssessmentDate).HasDatabaseName("IX_RiskAssessmentDate");
            builder.HasIndex(x => x.PlannedRiskAssessmentDate).HasDatabaseName("IX_PlannedRiskAssessmentDate");
            builder.HasIndex(x => x.LastChangedAt).HasDatabaseName("IX_LastChangedAt");
            builder.HasIndex(x => x.WebAccessibilityCompliance).HasDatabaseName("IX_WebAccessibilityCompliance");
            builder.HasIndex(x => x.LastWebAccessibilityCheck).HasDatabaseName("IX_LastWebAccessibilityCheck");

            builder.HasOne(t => t.Organization)
                .WithMany(t => t.ItSystemUsageOverviewReadModels)
                .HasForeignKey(d => d.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SourceEntity)
                .WithMany(x => x.OverviewReadModels)
                .HasForeignKey(x => x.SourceEntityId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.SourceEntityUuid).IsRequired();

            // LicensingAndCodeModels is persisted as a string but exposed as a collection via the entity property
            // Ignore the navigation property to prevent EF from trying to map it as an array
            builder.Ignore(x => x.LicensingAndCodeModels);
        }
    }
}
