using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class AppTypeMap : EntityTypeConfiguration<AppType>
    {
        public AppTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("AppType");
            this.Property(t => t.Id).HasColumnName("Id");

        }
    }
}