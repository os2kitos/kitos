using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ProjectPhaseLocaleMap : EntityTypeConfiguration<ProjPhaseLocale>
    {
        public ProjectPhaseLocaleMap()
        {
            // Primary Key
            this.HasKey(t => new { t.Municipality_Id, t.Original_Id });

            // Properties
            // Table & Column Mappings
            this.ToTable("ProjectPhaseLocale");

            // Relationships
            this.HasRequired(t => t.Municipality)
                .WithMany(t => t.ProjectPhaseLocales)
                .HasForeignKey(t => t.Municipality_Id);

            this.HasRequired(t => t.Original)
                .WithMany(t => t.Locales)
                .HasForeignKey(t => t.Original_Id);
        }
    }
}