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
            this.Property(t => t.ItSystemId).HasColumnName("ItSystemId");

            // Relationships
            this.HasOptional(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(d => d.ParentId)
                .WillCascadeOnDelete(false);
            this.HasOptional(t => t.ItProject)
                .WithMany(t => t.TaskRefs)
                .HasForeignKey(d => d.ItProjectId);
            /*this.HasOptional(t => t.ItSystem)
                .WithMany(t => t.KLEs)
                .HasForeignKey(d => d.ItSystemId);*/
            this.HasRequired(t => t.OwnedByOrganizationUnit)
                .WithMany(t => t.OwnedTasks)
                .HasForeignKey(d => d.OwnedByOrganizationUnitId);
        }
    }
}
