using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ExtRefTypeLocaleMap : EntityMap<ExtRefTypeLocale>
    {
        public ExtRefTypeLocaleMap()
        {
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