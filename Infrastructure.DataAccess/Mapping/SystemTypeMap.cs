using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class SystemTypeMap : EntityTypeConfiguration<SystemType>
    {
        public SystemTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("SystemType");
            this.Property(t => t.Id).HasColumnName("Id");

        }
    }
}