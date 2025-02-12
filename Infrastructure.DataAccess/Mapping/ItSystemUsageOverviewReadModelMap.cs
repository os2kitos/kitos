using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.Users;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOverviewReadModelMap : EntityTypeConfiguration<ItSystemUsageOverviewReadModel>
    {
        public ItSystemUsageOverviewReadModelMap()
        {
            Property(x => x.SystemName)
                .HasMaxLength(ItSystem.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_Name", 0);

            Property(x => x.SystemDescription)
                .IsOptional();

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
                .HasMaxLength(ItSystemUsage.LongProperyMaxLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_LocalSystemId", 0);

            Property(x => x.ItSystemUuid)
                .HasMaxLength(50)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemUuid", 0);

            Property(x => x.ParentItSystemUuid).IsOptional();

            Property(x => x.ParentItSystemUsageUuid).IsOptional()
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ParentItSystemUsageUuid", 0);

            Property(x => x.ParentItSystemName)
                .HasMaxLength(ItSystem.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemParentName", 0);

            Property(x => x.ResponsibleOrganizationUnitId)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationId", 0);

            Property(x => x.ResponsibleOrganizationUnitUuid)
                .IsOptional()
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationUuid", 0);

            Property(x => x.ResponsibleOrganizationUnitName)
                .HasMaxLength(OrganizationUnit.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationName", 0);

            Property(x => x.ItSystemBusinessTypeUuid)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeUuid", 0);

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

            Property(x => x.ItSystemCategoriesUuid)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesUuid", 0);

            Property(x => x.ItSystemCategoriesId)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesId", 0);

            Property(x => x.ItSystemCategoriesName)
                .HasMaxLength(Organization.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesName", 0);

            Property(x => x.LocalReferenceTitle)
                .HasMaxLength(ItSystemUsageOverviewReadModel.MaxReferenceTitleLenght)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_LocalReferenceTitle", 0);

            Property(x => x.ObjectOwnerId)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ObjectOwnerId", 0);

            Property(x => x.ObjectOwnerName)
                .HasMaxLength(UserConstraints.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ObjectOwnerName", 0);

            Property(x => x.LastChangedById)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_LastChangedById", 0);

            Property(x => x.LastChangedByName)
                .HasMaxLength(UserConstraints.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_LastChangedByName", 0);

            Property(x => x.MainContractId)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_MainContractId", 0);

            Property(x => x.MainContractSupplierId)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_MainContractSupplierId", 0);

            Property(x => x.MainContractSupplierName)
                .HasMaxLength(Organization.MaxNameLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_MainContractSupplierName", 0);

            Property(x => x.ArchiveDuty)
               .IsOptional()
               .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ArchiveDuty", 0);

            Property(x => x.ContainsAITechnology)
                .IsOptional();

            Property(x => x.IsHoldingDocument)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_IsHoldingDocument", 0);

            Property(x => x.RiskSupervisionDocumentationName)
                .HasMaxLength(ItSystemUsage.LinkNameMaxLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_RiskSupervisionDocumentationName", 0);

            Property(x => x.LinkToDirectoryName)
                .HasMaxLength(ItSystemUsage.LinkNameMaxLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_LinkToDirectoryName", 0);

            Property(x => x.GeneralPurpose)
                .HasMaxLength(ItSystemUsage.LongProperyMaxLength)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_GeneralPurpose", 0);

            Property(x => x.HostedAt)
               .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_HostedAt", 0);

            Property(x => x.UserCount)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_UserCount");

            Property(x => x.DPIAConducted)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_DPIAConducted");

            Property(x => x.IsBusinessCritical)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_IsBusinessCritical");

            Property(x => x.ActiveAccordingToValidityPeriod)
               .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ActiveAccordingToValidityPeriod", 0);

            Property(x => x.ActiveAccordingToLifeCycle)
               .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ActiveAccordingToLifeCycle", 0);

            Property(x => x.LifeCycleStatus)
               .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_LifeCycleStatus", 0);

            Property(x => x.Concluded)
                .IsOptional()
                .HasIndexAnnotation("IX_Concluded");

            Property(x => x.ExpirationDate)
                .IsOptional()
                .HasIndexAnnotation("IX_ExpirationDate");

            Property(x => x.SystemActive)
               .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_SystemActive", 0);

            Property(x => x.RiskAssessmentDate)
                .IsOptional()
               .HasIndexAnnotation("IX_RiskAssessmentDate", 0);

            Property(x => x.PlannedRiskAssessmentDate)
                .HasIndexAnnotation("IX_PlannedRiskAssessmentDate");

            Property(x => x.LastChangedAt)
                .HasIndexAnnotation("IX_LastChangedAt");

            //No index bc we don't know how long it might be
            Property(x => x.ItSystemKLEIdsAsCsv).IsOptional();
            Property(x => x.ItSystemKLENamesAsCsv).IsOptional();

            Property(x => x.LocalReferenceTitle).IsOptional();
            Property(x => x.LocalReferenceUrl).IsOptional();

            Property(x => x.SensitiveDataLevelsAsCsv).IsOptional();

            Property(x => x.RiskSupervisionDocumentationUrl).IsOptional();
            Property(x => x.LinkToDirectoryUrl).IsOptional();

            Property(x => x.DataProcessingRegistrationsConcludedAsCsv).IsOptional();
            Property(x => x.DataProcessingRegistrationNamesAsCsv).IsOptional();

            Property(x => x.DependsOnInterfacesNamesAsCsv).IsOptional();
            Property(x => x.IncomingRelatedItSystemUsagesNamesAsCsv).IsOptional();
            Property(x => x.OutgoingRelatedItSystemUsagesNamesAsCsv).IsOptional();

            HasRequired(t => t.Organization)
                .WithMany(t => t.ItSystemUsageOverviewReadModels)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.SourceEntity)
                .WithMany(x => x.OverviewReadModels)
                .HasForeignKey(x => x.SourceEntityId)
                .WillCascadeOnDelete(false);

            Property(x => x.SourceEntityUuid).IsRequired();
            Property(x => x.AssociatedContractsNamesCsv).IsOptional();
        }
    }
}
