using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.Text;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractGuidanceMap : EntityTypeConfiguration<ItContractGuidance>
    {
        public ItContractGuidanceMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ItContractGuidance");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Text).HasColumnName("Text");
        }
    }
}