using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class TsaMap : EntityTypeConfiguration<Tsa>
    {
        public TsaMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Tsa");
            this.Property(t => t.Id).HasColumnName("Id");
        }
    }
}