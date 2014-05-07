using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class StateMap : EntityTypeConfiguration<State>
    {
        public StateMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Table & Column Mappings
            this.ToTable("State");

            this.HasOptional(t => t.AssociatedUser)
                .WithMany(d => d.States)
                .HasForeignKey(t => t.AssociatedUserId)
                .WillCascadeOnDelete(false);

            this.HasOptional(t => t.AssociatedActivity)
                .WithMany(d => d.AssociatedStates)
                .HasForeignKey(t => t.AssociatedActivityId);
        }
    }
}