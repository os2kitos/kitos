using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItProjectStatusMap : EntityMap<ItProjectStatus>
    {
        public ItProjectStatusMap()
        {
            // Table & Column Mappings
            this.ToTable("ItProjectStatus");

            this.HasOptional(t => t.AssociatedUser)
                .WithMany(d => d.ItProjectStatuses)
                .HasForeignKey(t => t.AssociatedUserId)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.AssociatedItProject)
                .WithMany(t => t.ItProjectStatuses)
                .HasForeignKey(d => d.AssociatedItProjectId)
                .WillCascadeOnDelete(false);
        }
    }
}