using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class EnvironmentMap : EntityTypeConfiguration<Environment>
    {
        public EnvironmentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Environment");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}
