using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSupportConfigMap : EntityTypeConfiguration<ItSupportConfig>
    {
        public ItSupportConfigMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("ItSupportCfg");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.Municipality)
                .WithOptional(t => t.ItSupportConfig);
        }
    }
}