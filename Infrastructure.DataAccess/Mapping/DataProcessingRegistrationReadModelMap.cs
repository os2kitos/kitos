using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Users;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationReadModelMap : EntityTypeConfiguration<DataProcessingRegistrationReadModel>
    {
        public DataProcessingRegistrationReadModelMap()
        {
            Property(x => x.Name)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("DataProcessingRegistrationReadModel_Index_Name", 0);

            Property(x => x.MainReferenceTitle)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxReadmodelPropertyLength)
                .IsOptional()
                .HasIndexAnnotation("DataProcessingRegistrationReadModel_Index_MainReferenceTitle", 0);

            //No index of this, length is unknown since no bounds on system assignment.
            Property(x => x.SystemNamesAsCsv).IsOptional();
            Property(x => x.SystemUuidsAsCsv).IsOptional();

            Property(x => x.MainReferenceUserAssignedId).IsOptional();

            Property(x => x.MainReferenceUrl).IsOptional();

            HasRequired(t => t.Organization)
                .WithMany(t => t.DataProcessingRegistrationReadModels)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            HasRequired(x => x.SourceEntity)
                .WithMany(x => x.ReadModels)
                .HasForeignKey(x => x.SourceEntityId)
                .WillCascadeOnDelete(false);

            Property(x => x.SourceEntityUuid).IsRequired();

            //No index bc we don't know how long it might be
            Property(x => x.DataProcessorNamesAsCsv).IsOptional();
            Property(x => x.SubDataProcessorNamesAsCsv).IsOptional();

            Property(x => x.IsAgreementConcluded)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_Concluded", 0);

            Property(x => x.TransferToInsecureThirdCountries)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_TransferToInsecureThirdCountries", 0);

            Property(x => x.BasisForTransfer)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxReadmodelPropertyLength)
                .IsOptional()
                .HasIndexAnnotation("IX_DRP_BasisForTransfer", 0);

            Property(x => x.BasisForTransferUuid)
                .IsOptional()
                .HasIndexAnnotation("IX_DRP_BasisForTransferUuid", 0);

            Property(x => x.OversightInterval)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_OversightInterval", 0);


            Property(x => x.DataResponsible)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxReadmodelPropertyLength)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_DataResponsible", 0);

            Property(x => x.DataResponsibleUuid)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_DataResponsibleUuid", 0);

            Property(x => x.OversightOptionNamesAsCsv).IsOptional();

            Property(x => x.IsOversightCompleted)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_IsOversightCompleted", 0);

            Property(x => x.ContractNamesAsCsv).IsOptional();

            Property(x => x.LastChangedById)
                .HasIndexAnnotation("DataProcessingRegistrationReadModel_Index_LastChangedById", 0);

            Property(x => x.LastChangedByName)
                .HasMaxLength(UserConstraints.MaxNameLength)
                .HasIndexAnnotation("DataProcessingRegistrationReadModel_Index_LastChangedByName", 0);

            Property(x => x.LastChangedAt)
                .HasIndexAnnotation("DataProcessingRegistrationReadModel_Index_LastChangedAt", 0);

            Property(x => x.OversightScheduledInspectionDate)
                .HasIndexAnnotation("IX_DPR_OversightScheduledInspectionDate");

            Property(x => x.IsActive)
                .HasIndexAnnotation("IX_DPR_IsActive");

            Property(x => x.ActiveAccordingToMainContract)
                .HasIndexAnnotation("IX_DPR_MainContractIsActive");

            Property(x => x.ResponsibleOrgUnitUuid)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_ResponsibleOrgUnitUuid");

            Property(x => x.ResponsibleOrgUnitId)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_ResponsibleOrgUnitId");

            Property(x => x.ResponsibleOrgUnitName)
                .IsOptional();
        }
    }
}