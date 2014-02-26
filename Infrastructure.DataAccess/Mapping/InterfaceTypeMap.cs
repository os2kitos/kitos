using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class InterfaceTypeMap : EntityTypeConfiguration<InterfaceType>
    {
        public InterfaceTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("InterfaceType");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}