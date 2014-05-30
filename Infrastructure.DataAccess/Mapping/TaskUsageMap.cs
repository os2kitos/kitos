using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class TaskUsageMap : EntityMap<TaskUsage>
    {
        public TaskUsageMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("TaskUsage");

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
