using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationReadModelMap : IEntityTypeConfiguration<DataProcessingRegistrationReadModel>
    {
        public void Configure(EntityTypeBuilder<DataProcessingRegistrationReadModel> builder)
        {
            builder.Property(x => x.Name)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength)
                .IsRequired();
            builder.HasIndex(x => x.Name).HasDatabaseName("DataProcessingRegistrationReadModel_Index_Name");

            builder.Property(x => x.MainReferenceTitle)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxReadmodelPropertyLength);
            builder.HasIndex(x => x.MainReferenceTitle).HasDatabaseName("DataProcessingRegistrationReadModel_Index_MainReferenceTitle");

            //No index of this, length is unknown since no bounds on system assignment.
            builder.Property(x => x.SystemNamesAsCsv);
            builder.Property(x => x.SystemUuidsAsCsv);
            builder.Property(x => x.SystemValiditiesAsCsv);

            builder.Property(x => x.MainReferenceUserAssignedId);

            builder.Property(x => x.MainReferenceUrl);

            builder.HasOne(t => t.Organization)
                .WithMany(t => t.DataProcessingRegistrationReadModels)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SourceEntity)
                .WithMany(x => x.ReadModels)
                .HasForeignKey(x => x.SourceEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.SourceEntityUuid).IsRequired();

            //No index bc we don't know how long it might be
            builder.Property(x => x.DataProcessorNamesAsCsv);
            builder.Property(x => x.SubDataProcessorNamesAsCsv);

            builder.Property(x => x.IsAgreementConcluded);
            builder.HasIndex(x => x.IsAgreementConcluded).HasDatabaseName("IX_DPR_Concluded");

            builder.Property(x => x.TransferToInsecureThirdCountries);
            builder.HasIndex(x => x.TransferToInsecureThirdCountries).HasDatabaseName("IX_DPR_TransferToInsecureThirdCountries");

            builder.Property(x => x.BasisForTransfer)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxReadmodelPropertyLength);
            builder.HasIndex(x => x.BasisForTransfer).HasDatabaseName("IX_DRP_BasisForTransfer");

            builder.Property(x => x.BasisForTransferUuid);
            builder.HasIndex(x => x.BasisForTransferUuid).HasDatabaseName("IX_DRP_BasisForTransferUuid");

            builder.Property(x => x.OversightInterval);
            builder.HasIndex(x => x.OversightInterval).HasDatabaseName("IX_DPR_OversightInterval");

            builder.Property(x => x.DataResponsible)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxReadmodelPropertyLength);
            builder.HasIndex(x => x.DataResponsible).HasDatabaseName("IX_DPR_DataResponsible");

            builder.Property(x => x.DataResponsibleUuid);
            builder.HasIndex(x => x.DataResponsibleUuid).HasDatabaseName("IX_DPR_DataResponsibleUuid");

            builder.Property(x => x.OversightOptionNamesAsCsv);

            builder.Property(x => x.IsOversightCompleted);
            builder.HasIndex(x => x.IsOversightCompleted).HasDatabaseName("IX_DPR_IsOversightCompleted");

            builder.Property(x => x.ContractNamesAsCsv);

            builder.Property(x => x.LastChangedById);
            builder.HasIndex(x => x.LastChangedById).HasDatabaseName("DataProcessingRegistrationReadModel_Index_LastChangedById");

            builder.Property(x => x.LastChangedByName)
                .HasMaxLength(UserConstraints.MaxNameLength);
            builder.HasIndex(x => x.LastChangedByName).HasDatabaseName("DataProcessingRegistrationReadModel_Index_LastChangedByName");

            builder.Property(x => x.LastChangedAt);
            builder.HasIndex(x => x.LastChangedAt).HasDatabaseName("DataProcessingRegistrationReadModel_Index_LastChangedAt");

            builder.Property(x => x.OversightScheduledInspectionDate);
            builder.HasIndex(x => x.OversightScheduledInspectionDate).HasDatabaseName("IX_DPR_OversightScheduledInspectionDate");

            builder.Property(x => x.IsActive);
            builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_DPR_IsActive");

            builder.Property(x => x.ActiveAccordingToMainContract);
            builder.HasIndex(x => x.ActiveAccordingToMainContract).HasDatabaseName("IX_DPR_MainContractIsActive");

            builder.Property(x => x.ResponsibleOrgUnitUuid);
            builder.HasIndex(x => x.ResponsibleOrgUnitUuid).HasDatabaseName("IX_DPR_ResponsibleOrgUnitUuid");

            builder.Property(x => x.ResponsibleOrgUnitId);
            builder.HasIndex(x => x.ResponsibleOrgUnitId).HasDatabaseName("IX_DPR_ResponsibleOrgUnitId");

            builder.Property(x => x.ResponsibleOrgUnitName);
        }
    }
}