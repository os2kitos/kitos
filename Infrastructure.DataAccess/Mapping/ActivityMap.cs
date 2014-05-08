using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ActivityMap : EntityTypeConfiguration<Activity>
    {
        public ActivityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("Activity");

            this.HasOptional(t => t.AssociatedUser)
                .WithMany(d => d.Activities)
                .HasForeignKey(t => t.AssociatedUserId)
                .WillCascadeOnDelete(false);

            this.HasOptional(t => t.AssociatedActivity)
                .WithMany(d => d.AssociatedActivities)
                .HasForeignKey(t => t.AssociatedActivityId);

            this.HasRequired(t => t.ObjectOwner)
                .WithMany(d => d.CreatedActivities)
                .HasForeignKey(t => t.ObjectOwnerId);
        }
    }
}