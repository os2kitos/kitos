using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class DatabaseTypeMap : EntityTypeConfiguration<DatabaseType>
    {
        public DatabaseTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("DatabaseType");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}
