using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ProjectPhaseLocaleMap : EntityTypeConfiguration<ProjectPhaseLocale>
    {
        public ProjectPhaseLocaleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);
            //this.HasKey(t => new { t.Municipality, t.ProjectPhase});

            // Properties
            // Table & Column Mappings
            this.ToTable("ProjectPhaseLocale");

            // Relationships
            this.HasRequired(t => t.Municipality)
                .WithOptional(t => t.ProjectPhaseLocale);

            this.HasRequired(t => t.ProjectPhase)
                .WithOptional(t => t.ProjectPhaseLocale);
        }
    }
}