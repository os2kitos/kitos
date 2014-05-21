using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class ProcurementPlanMap : EntityTypeConfiguration<ProcurementPlan>
    {
        public ProcurementPlanMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ProcurementPlan");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}