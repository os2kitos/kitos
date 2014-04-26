using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class SensitiveDataTypeMap : EntityTypeConfiguration<SensitiveDataType>
    {
        public SensitiveDataTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("SensitiveDataType");
        }
    }
}