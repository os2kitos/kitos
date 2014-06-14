using System.Data.Entity.ModelConfiguration;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class StateMap : EntityMap<State>
    {
        public StateMap()
        {
            // Table & Column Mappings
            this.ToTable("State");

            this.HasOptional(t => t.AssociatedUser)
                .WithMany(d => d.States)
                .HasForeignKey(t => t.AssociatedUserId)
                .WillCascadeOnDelete(false);
        }
    }
}