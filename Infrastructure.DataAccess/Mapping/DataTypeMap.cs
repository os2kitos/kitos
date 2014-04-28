using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class DataTypeMap : EntityTypeConfiguration<DataType>
    {
        public DataTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("DataType");
            this.Property(t => t.Id).HasColumnName("Id");

        }
    }
}