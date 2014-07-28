using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class AssignmentMap : EntityMap<Assignment>
    {
        public AssignmentMap()
        {
            // Table & Column Mappings
            this.ToTable("Assignment");

            //this.HasOptional(t => t.AssociatedUser)
            //    .WithMany(d => d.Activities)
            //    .HasForeignKey(t => t.AssociatedUserId)
            //    .WillCascadeOnDelete(false);
        }
    }
}