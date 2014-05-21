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
            

            // Relationships
            this.HasRequired(t => t.ObjectOwner)
                .WithMany(t => t.CreatedItContracts)
                .HasForeignKey(d => d.ObjectOwnerId)
                .WillCascadeOnDelete(false);

            this.HasOptional(t => t.ContractTemplate)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ContractTemplateId);

            this.HasOptional(t => t.ContractType)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ContractTypeId);
            
            this.HasOptional(t => t.PurchaseForm)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.PurchaseFormId);

            this.HasOptional(t => t.Supplier)
                .WithMany(t => t.ItContracts)
                .HasForeignKey(d => d.SupplierId);

            this.HasOptional(t => t.ProcurementPlan)
                .WithRequired(t => t.ItContract)
                .WillCascadeOnDelete(true);

            this.HasOptional(t => t.ProcurementStrategy)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ProcurementStrategyId);

            this.HasOptional(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(d => d.ParentId)
                .WillCascadeOnDelete(false);

            this.HasMany(t => t.AgreementElements)
                .WithMany(t => t.References);
        }
    }
}
