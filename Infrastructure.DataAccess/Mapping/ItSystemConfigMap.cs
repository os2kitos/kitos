using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemConfigMap : EntityTypeConfiguration<ItSystemConfig>
    {
        public ItSystemConfigMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("ItSystemCfg");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.Municipality)
                .WithOptional(t => t.ItSystemConfig);
        }
    }
}