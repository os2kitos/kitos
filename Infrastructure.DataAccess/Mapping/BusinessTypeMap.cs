using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class BusinessTypeMap : EntityTypeConfiguration<BusinessType>
    {
        public BusinessTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("BusinessType");
            this.Property(t => t.Id).HasColumnName("Id");

        }
    }
}