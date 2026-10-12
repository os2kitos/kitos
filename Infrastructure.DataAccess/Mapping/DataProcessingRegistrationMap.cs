using Core.DomainModel.GDPR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataProcessingRegistrationMap : IEntityTypeConfiguration<DataProcessingRegistration>
    {
        public void Configure(EntityTypeBuilder<DataProcessingRegistration> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(DataProcessingRegistrationConstraints.MaxNameLength).IsRequired();

            builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique().HasDatabaseName("UX_NameUniqueToOrg");
            builder.HasIndex(x => x.OrganizationId).HasDatabaseName("IX_OrganizationId");
            builder.HasIndex(x => x.Name).HasDatabaseName("IX_Name");

            builder.HasOne(t => t.Organization)
                .WithMany(t => t.DataProcessingRegistrations)
                .HasForeignKey(d => d.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.ExternalReferences)
                .WithOne(d => d.DataProcessingRegistration)
                .HasForeignKey(d => d.DataProcessingRegistration_Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.SystemUsages)
                .WithMany(x => x.AssociatedDataProcessingRegistrations)
                .UsingEntity(
                    l => l.HasOne(typeof(Core.DomainModel.ItSystemUsage.ItSystemUsage)).WithMany().HasForeignKey("ItSystemUsage_Id"),
                    r => r.HasOne(typeof(DataProcessingRegistration)).WithMany().HasForeignKey("DataProcessingRegistration_Id"),
                    j => { j.ToTable("DataProcessingRegistrationItSystemUsages"); j.HasKey("DataProcessingRegistration_Id", "ItSystemUsage_Id"); });

            builder.HasMany(x => x.DataProcessors)
                .WithMany(x => x.DataProcessorForDataProcessingRegistrations)
                .UsingEntity(
                    l => l.HasOne(typeof(Core.DomainModel.Organization.Organization)).WithMany().HasForeignKey("Organization_Id"),
                    r => r.HasOne(typeof(DataProcessingRegistration)).WithMany().HasForeignKey("DataProcessingRegistration_Id"),
                    j => { j.ToTable("DataProcessingRegistrationOrganizations"); j.HasKey("DataProcessingRegistration_Id", "Organization_Id"); });

            builder.HasMany(x => x.InsecureCountriesSubjectToDataTransfer)
                .WithMany(x => x.References)
                .UsingEntity(
                    l => l.HasOne(typeof(DataProcessingCountryOption)).WithMany().HasForeignKey("DataProcessingCountryOption_Id"),
                    r => r.HasOne(typeof(DataProcessingRegistration)).WithMany().HasForeignKey("DataProcessingRegistration_Id"),
                    j => { j.ToTable("DataProcessingRegistrationDataProcessingCountryOptions"); j.HasKey("DataProcessingRegistration_Id", "DataProcessingCountryOption_Id"); });

            builder.HasOne(x => x.BasisForTransfer)
                .WithMany(x => x.References)
                .HasForeignKey(x => x.BasisForTransferId);

            builder.HasOne(x => x.DataResponsible)
                .WithMany(x => x.References)
                .HasForeignKey(x => x.DataResponsible_Id);

            builder.HasMany(x => x.OversightOptions)
                .WithMany(x => x.References)
                .UsingEntity(
                    l => l.HasOne(typeof(DataProcessingOversightOption)).WithMany().HasForeignKey("DataProcessingOversightOption_Id"),
                    r => r.HasOne(typeof(DataProcessingRegistration)).WithMany().HasForeignKey("DataProcessingRegistration_Id"),
                    j => { j.ToTable("DataProcessingRegistrationDataProcessingOversightOptions"); j.HasKey("DataProcessingRegistration_Id", "DataProcessingOversightOption_Id"); });

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_DataProcessingRegistration_Uuid");

            builder.HasOne(x => x.ResponsibleOrganizationUnit)
                .WithMany(x => x.ResponsibleForDataProcessingRegistrations)
                .HasForeignKey(t => t.ResponsibleOrganizationUnitId);

            builder.Property(x => x.EnforceInvalidity).HasColumnType("boolean");
        }
    }
}
