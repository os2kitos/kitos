using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class GoalMap : EntityMap<Goal>
    {
        public GoalMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("Goal");
            this.Property(t => t.GoalStatusId).HasColumnName("GoalStatusId");

            // Relationships
            this.HasRequired(t => t.GoalStatus)
                .WithMany(t => t.Goals)
                .HasForeignKey(d => d.GoalStatusId);

            this.HasOptional(t => t.GoalType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.GoalTypeId);
        }
    }
}
