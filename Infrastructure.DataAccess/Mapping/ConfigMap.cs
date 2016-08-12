using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ConfigMap : EntityMap<Config>
    {
        public ConfigMap()
        {
            // Table & Column Mappings
            ToTable("Config");
            Property(t => t.Id).HasColumnName("Id");

            // Relationships
            HasRequired(t => t.Organization)
                .WithOptional(t => t.Config);
        }
    }
}
