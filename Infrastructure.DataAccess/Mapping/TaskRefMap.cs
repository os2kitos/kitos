using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class TaskRefMap : EntityTypeConfiguration<TaskRef>
    {
        public TaskRefMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TaskRef");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ItProjectId).HasColumnName("ItProjectId");

            // Relationships
            this.HasOptional(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(d => d.ParentId)
                .WillCascadeOnDelete(false);
            //this.HasOptional(t => t.ItProject)
            //    .WithMany(t => t.TaskRefs)
            //    .HasForeignKey(d => d.ItProjectId);
            this.HasRequired(t => t.OwnedByOrganizationUnit)
                .WithMany(t => t.OwnedTasks)
                .HasForeignKey(d => d.OwnedByOrganizationUnitId);
            this.HasMany(t => t.ItSystems)
                .WithMany(t => t.TaskRefs);
            this.HasMany(t => t.ItSystemUsages)
                .WithMany(t => t.TaskRefs);
        }
    }
}
