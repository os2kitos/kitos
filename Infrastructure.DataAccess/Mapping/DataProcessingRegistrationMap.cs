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
                .IsRequired();

            HasIndex(x => new { x.OrganizationId, x.Name })
                .IsUnique(true)
                .HasName("UX_NameUniqueToOrg");

            HasIndex(x => x.OrganizationId)
                .IsUnique(false)
                .HasName("IX_OrganizationId");

            HasIndex(x => x.Name)
                .IsUnique(false)
                .HasName("IX_Name");

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

            //Transfer to insecure countries
            HasMany(x => x.InsecureCountriesSubjectToDataTransfer)
                .WithMany(x => x.References);
            Property(x => x.TransferToInsecureThirdCountries).IsOptional();

            //Basis for transfer
            HasOptional(x => x.BasisForTransfer)
                .WithMany(x => x.References)
                .HasForeignKey(x => x.BasisForTransferId);

            //Data responsible
            HasOptional(x => x.DataResponsible)
                .WithMany(x => x.References)
                .HasForeignKey(x => x.DataResponsible_Id);

            //Oversight options
            HasMany(x => x.OversightOptions)
                .WithMany(x => x.References);

            Property(x => x.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_DataProcessingRegistration_Uuid", 0);

            HasOptional(x => x.ResponsibleOrganizationUnit)
                .WithMany(x => x.ResponsibleForDataProcessingRegistrations)
                .HasForeignKey(t => t.ResponsibleOrganizationUnitId);
        }
    }
}