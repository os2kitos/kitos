using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.GDPR;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationMap : EntityTypeConfiguration<DataProcessingRegistration>
    {
        public DataProcessingRegistrationMap()
        {
            //Simple properties
            Property(x => x.Name)
                .HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength)
                .IsRequired()
                .HasIndexAnnotation("DataProcessingRegistration_Index_Name", 0);

            Property(x => x.HasSubDataProcessors).IsOptional();

            //Organization relationship
            HasRequired(t => t.Organization)
                .WithMany(t => t.DataProcessingRegistrations)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            //External references
            HasOptional(t => t.Reference);
            HasMany(t => t.ExternalReferences)
                .WithOptional(d => d.DataProcessingRegistration)
                .HasForeignKey(d => d.DataProcessingRegistration_Id)
                .WillCascadeOnDelete(true);

            //It-systems
            HasMany(x => x.SystemUsages)
                .WithMany(x => x.AssociatedDataProcessingRegistrations);

            //Data processors
            HasMany(x => x.DataProcessors)
                .WithMany(x => x.DataProcessorForDataProcessingRegistrations);

            //Sub Data processors
            HasMany(x => x.SubDataProcessors)
                .WithMany(x => x.SubDataProcessorForDataProcessingRegistrations);

            //Transfer to insecure countries
            HasMany(x => x.InsecureCountriesSubjectToDataTransfer)
                .WithMany(x => x.InsecureDataTransferSubjectsInDataProcessingRegistrations);
            Property(x => x.TransferToInsecureThirdCountries).IsOptional();

            //Basis for transfer
            HasOptional(x => x.BasisForTransfer)
                .WithMany(x => x.References)
                .HasForeignKey(x => x.BasisForTransferId);

            //Data responsible
            HasOptional(x => x.DataResponsible)
                .WithMany(x => x.References)
                .HasForeignKey(x => x.DataResponsible_Id);
        }
    }
}