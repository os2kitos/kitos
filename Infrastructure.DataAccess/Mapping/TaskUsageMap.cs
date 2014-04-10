using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class TaskUsageMap : EntityTypeConfiguration<TaskUsage>
    {
        public TaskUsageMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("TaskUsage");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.OrgUnit)
                .WithMany(o => o.TaskUsages)
                .HasForeignKey(t => t.OrgUnitId);

            this.HasRequired(t => t.TaskRef)
                .WithMany(r => r.Usages)
                .HasForeignKey(t => t.TaskRefId);
        }
    }
}
