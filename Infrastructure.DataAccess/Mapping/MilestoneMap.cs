using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class MilestoneMap : EntityTypeConfiguration<Milestone>
    {
        public MilestoneMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Milestone");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ProjectStatus_Id).HasColumnName("ProjectStatus_Id");

            // Relationships
            this.HasRequired(t => t.ProjectStatus)
                .WithMany(t => t.Milestones)
                .HasForeignKey(d => d.ProjectStatus_Id);

        }
    }
}
