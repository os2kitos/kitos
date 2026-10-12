using Core.DomainModel.ItContract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractMap : IEntityTypeConfiguration<ItContract>
    {
        public void Configure(EntityTypeBuilder<ItContract> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(ItContractConstraints.MaxNameLength).IsRequired();

            builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique().HasDatabaseName("UX_NameUniqueToOrg");
            builder.HasIndex(x => x.OrganizationId).HasDatabaseName("IX_OrganizationId");
            builder.HasIndex(x => x.Name).HasDatabaseName("IX_Name");
            builder.HasIndex(x => x.ProcurementInitiated).HasDatabaseName("IX_ProcurementInitiated");

            builder.ToTable("ItContract");

            builder.HasMany(t => t.ExternalReferences)
                .WithOne(d => d.ItContract)
                .HasForeignKey(d => d.Itcontract_Id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.ContractTemplate)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ContractTemplateId);

            builder.HasOne(t => t.ContractType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ContractTypeId);

            builder.HasOne(t => t.PurchaseForm)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.PurchaseFormId);

            builder.HasOne(t => t.Supplier)
                .WithMany(t => t.Supplier)
                .HasForeignKey(d => d.SupplierId);

            builder.HasOne(t => t.ProcurementStrategy)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ProcurementStrategyId);

            builder.HasOne(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Criticality)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.CriticalityId);

            builder.HasMany(t => t.AssociatedAgreementElementTypes)
                .WithOne(t => t.ItContract)
                .HasForeignKey(t => t.ItContract_Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.ResponsibleOrganizationUnit)
                .WithMany(t => t.ResponsibleForItContracts)
                .HasForeignKey(d => d.ResponsibleOrganizationUnitId);

            builder.HasMany(t => t.AssociatedSystemUsages)
                .WithOne(t => t.ItContract)
                .HasForeignKey(d => d.ItContractId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.Organization)
                .WithMany(t => t.ItContracts)
                .HasForeignKey(d => d.OrganizationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.DataProcessingRegistrations)
                .WithMany(x => x.AssociatedContracts)
                .UsingEntity(
                    l => l.HasOne(typeof(Core.DomainModel.GDPR.DataProcessingRegistration)).WithMany().HasForeignKey("DataProcessingRegistration_Id"),
                    r => r.HasOne(typeof(ItContract)).WithMany().HasForeignKey("ItContract_Id"),
                    j => { j.ToTable("ItContractDataProcessingRegistrations"); j.HasKey("ItContract_Id", "DataProcessingRegistration_Id"); });

            builder.HasMany(t => t.DataProcessingRegistrationsWhereContractIsMainContract)
                .WithOne(t => t.MainContract)
                .HasForeignKey(d => d.MainContractId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_Contract_Uuid");
        }
    }
}
