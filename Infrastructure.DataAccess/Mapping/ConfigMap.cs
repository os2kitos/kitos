using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ConfigMap : EntityMap<Config>
    {
        public ConfigMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("Config");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.Organization)
                .WithOptional(t => t.Config);
        }
    }
}
