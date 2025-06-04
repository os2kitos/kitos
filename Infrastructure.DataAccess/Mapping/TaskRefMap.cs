using Core.DomainModel.Organization;

namespace Infrastructure.DataAccess.Mapping
{
    public class TaskRefMap : EntityMap<TaskRef>
    {
        public TaskRefMap()
        {
            // Properties
            // Table & Column Mappings
            ToTable("TaskRef");

            // Relationships
            HasOptional(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(d => d.ParentId)
                .WillCascadeOnDelete(false);

            HasRequired(t => t.OwnedByOrganizationUnit)
                .WithMany(t => t.OwnedTasks)
                .HasForeignKey(d => d.OwnedByOrganizationUnitId);

            HasMany(t => t.ItSystems)
                .WithMany(t => t.TaskRefs);

            HasMany(t => t.ItSystemUsages)
                .WithMany(t => t.TaskRefs);

            HasMany(t => t.ItSystemUsagesOptOut)
                .WithMany(t => t.TaskRefsOptOut)
                .Map(t =>
                {
                    t.ToTable("TaskRefItSystemUsageOptOut");
                });

            Property(x => x.TaskKey)
                .HasMaxLength(TaskRef.MaxTaskKeyLength)
                .HasUniqueIndexAnnotation("UX_TaskKey", 0);

            Property(x => x.Uuid)
                .IsRequired()
                .HasUniqueIndexAnnotation("UX_TaskRef_Uuid", 1);

            Property(x => x.Description)
                .HasMaxLength(TaskRef.MaxDescriptionLength);

        }
    }
}
