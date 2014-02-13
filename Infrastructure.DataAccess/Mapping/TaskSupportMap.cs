using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class TaskSupportMap : EntityTypeConfiguration<TaskSupport>
    {
        public TaskSupportMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TaskSupport");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ItSystem_Id).HasColumnName("ItSystem_Id");

            // Relationships
            this.HasRequired(t => t.ItSystem)
                .WithMany(t => t.TaskSupports)
                .HasForeignKey(d => d.ItSystem_Id);

        }
    }
}
