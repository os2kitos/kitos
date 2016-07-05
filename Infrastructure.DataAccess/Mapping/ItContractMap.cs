using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractMap : EntityTypeConfiguration<ItContract>
    {
        public ItContractMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("ItContract");

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
                .WithMany(t => t.Supplier)
                .HasForeignKey(d => d.SupplierId);

            this.HasOptional(t => t.ProcurementStrategy)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ProcurementStrategyId);

            this.HasOptional(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(d => d.ParentId)
                .WillCascadeOnDelete(false);

            this.HasMany(t => t.AgreementElements)
                .WithMany(t => t.References)
                .Map(mc =>
                    {
                        // have to rename key else it's too long for MySql
                        mc.MapLeftKey("ItContractId");
                        mc.MapRightKey("ElemId");
                    });

            this.HasOptional(t => t.ResponsibleOrganizationUnit)
                .WithMany(t => t.ResponsibleForItContracts)
                .HasForeignKey(d => d.ResponsibleOrganizationUnitId);

            this.HasMany(t => t.AssociatedSystemUsages)
                .WithRequired(t => t.ItContract)
                .HasForeignKey(d => d.ItContractId);

            this.HasRequired(t => t.Organization)
                .WithMany(t => t.ItContracts)
                .HasForeignKey(d => d.OrganizationId)
                .WillCascadeOnDelete(false);

            this.HasOptional(t => t.ContractSigner)
                .WithMany(d => d.SignerForContracts)
                .HasForeignKey(t => t.ContractSignerId);
        }
    }
}
