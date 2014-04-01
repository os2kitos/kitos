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
            this.Property(t => t.ItProject_Id).HasColumnName("ItProject_Id");
            this.Property(t => t.ItSystem_Id).HasColumnName("ItSystem_Id");

            // Relationships
            this.HasOptional(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(d => d.Parent_Id)
                .WillCascadeOnDelete(false);
            this.HasOptional(t => t.ItProject)
                .WithMany(t => t.TaskRefs)
                .HasForeignKey(d => d.ItProject_Id);
            this.HasOptional(t => t.ItSystem)
                .WithMany(t => t.KLEs)
                .HasForeignKey(d => d.ItSystem_Id);

        }
    }
}
