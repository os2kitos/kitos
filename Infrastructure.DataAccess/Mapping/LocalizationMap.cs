using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class LocalizationMap : EntityTypeConfiguration<Localization>
    {
        public LocalizationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Localization");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships

        }
    }
}
