using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ProtocolTypeMap : EntityTypeConfiguration<ProtocolType>
    {
        public ProtocolTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ProtocolType");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}