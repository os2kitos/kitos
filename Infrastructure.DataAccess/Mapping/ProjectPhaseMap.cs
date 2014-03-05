using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ProjectPhaseMap : EntityTypeConfiguration<ProjectPhase>
    {
        public ProjectPhaseMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ProjectPhase");
            this.Property(t => t.Id).HasColumnName("Id");

        }
    }
}