using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class ProcurementStrategyMap : EntityTypeConfiguration<ProcurementStrategy>
    {
        public ProcurementStrategyMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ProcurementStrategy");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}