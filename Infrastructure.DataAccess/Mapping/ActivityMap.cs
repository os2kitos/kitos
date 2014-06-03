using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ActivityMap : EntityMap<Activity>
    {
        public ActivityMap()
        {
            // Table & Column Mappings
            this.ToTable("Activity");

            this.HasOptional(t => t.AssociatedUser)
                .WithMany(d => d.Activities)
                .HasForeignKey(t => t.AssociatedUserId)
                .WillCascadeOnDelete(false);

            this.HasOptional(t => t.AssociatedActivity)
                .WithMany(d => d.AssociatedActivities)
                .HasForeignKey(t => t.AssociatedActivityId);
        }
    }
}