using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;

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

            Property(x => x.OversightInterval)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_OversightInterval", 0);


            Property(x => x.DataResponsible)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxReadmodelPropertyLength)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_DataResponsible", 0);

            Property(x => x.OversightOptionNamesAsCsv).IsOptional();

            Property(x => x.IsOversightCompleted)
                .IsOptional()
                .HasIndexAnnotation("IX_DPR_IsOversightCompleted", 0);

            Property(x => x.ContractNamesAsCsv).IsOptional();

        }
    }
}