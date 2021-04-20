using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Core.DomainModel.Users;

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

            Property(x => x.HasMainContract)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_HasMainContract", 0);

            Property(x => x.ArchiveDuty)
               .IsOptional()
               .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_ArchiveDuty", 0);

            Property(x => x.IsHoldingDocument)
                .HasIndexAnnotation("ItSystemUsageOverviewReadModel_Index_IsHoldingDocument", 0);

            Property(x => x.RiskSupervisionDocumentationName)
                .HasMaxLength(ItSystemUsage.LinkNameMaxLength)
                .HasIndexAnnotation("ItSystemUsage_Index_RiskSupervisionDocumentationName", 0);

            Property(x => x.LinkToDirectoryName)
                .HasMaxLength(ItSystemUsage.LinkNameMaxLength)
                .HasIndexAnnotation("ItSystemUsage_Index_LinkToDirectoryName", 0);



            //No index bc we don't know how long it might be
            Property(x => x.ItSystemKLEIdsAsCsv).IsOptional();
            Property(x => x.ItSystemKLENamesAsCsv).IsOptional();

            Property(x => x.LocalReferenceTitle).IsOptional();
            Property(x => x.LocalReferenceUrl).IsOptional();

            Property(x => x.SensitiveDataLevelsAsCsv).IsOptional();

            Property(x => x.ItProjectNamesAsCsv).IsOptional();

            Property(x => x.RiskSupervisionDocumentationUrl).IsOptional();
            Property(x => x.LinkToDirectoryUrl).IsOptional();

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
