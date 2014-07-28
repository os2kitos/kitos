using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class MilestoneMap : EntityMap<Milestone>
    {
        public MilestoneMap()
        {
            // Table & Column Mappings
            this.ToTable("Milestone");

            //this.HasOptional(t => t.AssociatedUser)
            //    .WithMany(d => d.States)
            //    .HasForeignKey(t => t.AssociatedUserId)
            //    .WillCascadeOnDelete(false);
        }
    }
}