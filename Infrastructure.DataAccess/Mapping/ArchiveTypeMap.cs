using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ArchiveTypeMap : EntityTypeConfiguration<ArchiveType>
    {
        public ArchiveTypeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("ArchiveType");
        }
    }
}