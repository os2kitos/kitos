using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ProjectStatusMap : EntityTypeConfiguration<ProjectStatus>
    {
        public ProjectStatusMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ProjectStatus");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            //this.HasRequired(t => t.ItProject)
            //    .WithOptional(t => t.ProjectStatus);

            this.HasOptional(t => t.ProjectPhase)
                .WithMany(t => t.References)
                .HasForeignKey(d => d.ProjectPhaseId);
        }
    }
}
