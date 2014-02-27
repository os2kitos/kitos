using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ProjectPhaseLocaleMap : EntityTypeConfiguration<ProjectPhaseLocale>
    {
        public ProjectPhaseLocaleMap()
        {
            // Primary Key
            this.HasKey(t => new { t.Municipality_Id, t.ProjectPhase_Id });

            // Properties
            // Table & Column Mappings
            this.ToTable("ProjectPhaseLocale");

            // Relationships
            this.HasRequired(t => t.Municipality)
                .WithMany(t => t.ProjectPhaseLocales)
                .HasForeignKey(t => t.Municipality_Id);

        }
    }
}