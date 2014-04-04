using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractMap : EntityTypeConfiguration<ItContract>
    {
        public ItContractMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ItContract");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ContractTypeId).HasColumnName("ContractTypeId");
            this.Property(t => t.ContractTemplateId).HasColumnName("ContractTemplateId");
            this.Property(t => t.PurchaseFormId).HasColumnName("PurchaseFormId");
            this.Property(t => t.PaymentModelId).HasColumnName("PaymentModelId");
            this.Property(t => t.SupplierId).HasColumnName("SupplierId");
            this.Property(t => t.MunicipalityId).HasColumnName("MunicipalityId");

            // Relationships
            this.HasRequired(t => t.ContractTemplate)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ContractTemplateId);
            this.HasRequired(t => t.ContractType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ContractTypeId);
            this.HasRequired(t => t.PaymentModel)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.PaymentModelId);
            this.HasRequired(t => t.PurchaseForm)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.PurchaseFormId);
            this.HasRequired(t => t.Supplier)
                .WithMany(t => t.ItContracts)
                .HasForeignKey(d => d.SupplierId);
            this.HasRequired(t => t.Organization)
                .WithMany(t => t.ItContracts)
                .HasForeignKey(d => d.MunicipalityId)
                .WillCascadeOnDelete(false);
        }
    }
}
