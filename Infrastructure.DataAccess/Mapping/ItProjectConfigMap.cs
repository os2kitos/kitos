using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItProjectConfigMap : EntityTypeConfiguration<ItProjectConfig>
    {
        public ItProjectConfigMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("ItProjectCfg");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.Municipality)
                .WithOptional(t => t.ItProjectConfig);
        }
    }
}