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
                .WillCascadeOnDelete(true);

            this.HasMany(t => t.AssociatedSystems)
                .WithMany(d => d.Contracts);
        }
    }
}
