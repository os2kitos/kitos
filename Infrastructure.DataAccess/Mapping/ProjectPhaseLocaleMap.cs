using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ProjectPhaseLocaleMap : EntityTypeConfiguration<ProjPhaseLocale>
    {
        public ProjectPhaseLocaleMap()
        {
            // Primary Key
            this.HasKey(t => new { t.MunicipalityId, t.OriginalId });

            // Properties
            // Table & Column Mappings
            this.ToTable("ProjectPhaseLocale");

            // Relationships
            this.HasRequired(t => t.Organization)
                .WithMany(t => t.ProjectPhaseLocales)
                .HasForeignKey(t => t.MunicipalityId);

            this.HasRequired(t => t.Original)
                .WithMany(t => t.Locales)
                .HasForeignKey(t => t.OriginalId);
        }
    }
}