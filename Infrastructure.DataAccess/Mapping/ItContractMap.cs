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
            this.Property(t => t.ContractType_Id).HasColumnName("ContractType_Id");
            this.Property(t => t.ContractTemplate_Id).HasColumnName("ContractTemplate_Id");
            this.Property(t => t.PurchaseForm_Id).HasColumnName("PurchaseForm_Id");
            this.Property(t => t.PaymentModel_Id).HasColumnName("PaymentModel_Id");
            this.Property(t => t.Supplier_Id).HasColumnName("Supplier_Id");

            // Relationships
            this.HasRequired(t => t.ContractTemplate)
                .WithMany(t => t.ItContracts)
                .HasForeignKey(d => d.ContractTemplate_Id);
            this.HasRequired(t => t.ContractType)
                .WithMany(t => t.ItContracts)
                .HasForeignKey(d => d.ContractType_Id);
            this.HasRequired(t => t.PaymentModel)
                .WithMany(t => t.ItContracts)
                .HasForeignKey(d => d.PaymentModel_Id);
            this.HasRequired(t => t.PurchaseForm)
                .WithMany(t => t.ItContracts)
                .HasForeignKey(d => d.PurchaseForm_Id);
            this.HasRequired(t => t.Supplier)
                .WithMany(t => t.ItContracts)
                .HasForeignKey(d => d.Supplier_Id);

        }
    }
}
