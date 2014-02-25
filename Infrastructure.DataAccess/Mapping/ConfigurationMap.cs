using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ConfigurationMap : EntityTypeConfiguration<Configuration>
    {
        public ConfigurationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.IsSelected)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Configuration");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.IsSelected).HasColumnName("IsSelected");

        }
    }
}
