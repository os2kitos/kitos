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
            this.Property(t => t.Value)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("Localization");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Value).HasColumnName("Value");
            this.Property(t => t.Municipality_Id).HasColumnName("Municipality_Id");

            // Relationships
            this.HasRequired(t => t.Municipality)
                .WithMany(t => t.Localizations)
                .HasForeignKey(d => d.Municipality_Id);

        }
    }
}
