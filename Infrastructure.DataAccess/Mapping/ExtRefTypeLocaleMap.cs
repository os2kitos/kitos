using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ExtRefTypeLocaleMap : EntityTypeConfiguration<ExtRefTypeLocale>
    {
        public ExtRefTypeLocaleMap()
        {
            // Primary Key
            this.HasKey(t => new { t.MunicipalityId, t.OriginalId });

            // Properties
            // Table & Column Mappings
            this.ToTable("ExtRefTypeLocale");

            // Relationships
            this.HasRequired(t => t.Organization)
                .WithMany(t => t.ExtRefTypeLocales)
                .HasForeignKey(t => t.MunicipalityId);

            this.HasRequired(t => t.Original)
                .WithMany(t => t.Locales)
                .HasForeignKey(t => t.OriginalId);
        }
    }
}