using Core.DomainModel.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class TaskRefMap : EntityMap<TaskRef>
    {
        public override void Configure(EntityTypeBuilder<TaskRef> builder)
        {
            base.Configure(builder);
            base.Configure(builder);
            builder.ToTable("TaskRef");

            builder.HasOne(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.OwnedByOrganizationUnit)
                .WithMany(t => t.OwnedTasks)
                .HasForeignKey(d => d.OwnedByOrganizationUnitId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.ItSystems)
                .WithMany(t => t.TaskRefs)
                .UsingEntity(
                    l => l.HasOne(typeof(Core.DomainModel.ItSystem.ItSystem)).WithMany().HasForeignKey("ItSystem_Id"),
                    r => r.HasOne(typeof(TaskRef)).WithMany().HasForeignKey("TaskRef_Id"),
                    j => { j.ToTable("TaskRefItSystems"); j.HasKey("ItSystem_Id", "TaskRef_Id"); });

            builder.HasMany(t => t.ItSystemUsages)
                .WithMany(t => t.TaskRefs)
                .UsingEntity(
                    l => l.HasOne(typeof(Core.DomainModel.ItSystemUsage.ItSystemUsage)).WithMany().HasForeignKey("ItSystemUsage_Id"),
                    r => r.HasOne(typeof(TaskRef)).WithMany().HasForeignKey("TaskRef_Id"),
                    j => { j.ToTable("TaskRefItSystemUsages"); j.HasKey("ItSystemUsage_Id", "TaskRef_Id"); });

            builder.HasMany(t => t.ItSystemUsagesOptOut)
                .WithMany(t => t.TaskRefsOptOut)
                .UsingEntity(
                    l => l.HasOne(typeof(Core.DomainModel.ItSystemUsage.ItSystemUsage)).WithMany().HasForeignKey("ItSystemUsage_Id").OnDelete(DeleteBehavior.Cascade),
                    r => r.HasOne(typeof(TaskRef)).WithMany().HasForeignKey("TaskRef_Id").OnDelete(DeleteBehavior.Cascade),
                    j => { j.ToTable("TaskRefItSystemUsageOptOut"); j.HasKey("ItSystemUsage_Id", "TaskRef_Id"); });

            builder.Property(x => x.TaskKey)
                .HasMaxLength(TaskRef.MaxTaskKeyLength);
            builder.HasIndex(x => x.TaskKey).IsUnique().HasDatabaseName("UX_TaskKey");

            builder.Property(x => x.Uuid).IsRequired();
            builder.HasIndex(x => x.Uuid).IsUnique().HasDatabaseName("UX_TaskRef_Uuid");

            builder.Property(x => x.Description)
                .HasMaxLength(TaskRef.MaxDescriptionLength);
        }
    }
}
