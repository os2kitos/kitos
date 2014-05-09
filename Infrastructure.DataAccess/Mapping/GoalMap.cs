using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class GoalMap : EntityTypeConfiguration<Goal>
    {
        public GoalMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("Goal");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.GoalStatusId).HasColumnName("GoalStatusId");

            // Relationships
            this.HasRequired(t => t.GoalStatus)
                .WithMany(t => t.Goals)
                .HasForeignKey(d => d.GoalStatusId);

            this.HasRequired(t => t.GoalType)
                .WithMany(d => d.References)
                .HasForeignKey(t => t.GoalTypeId);

        }
    }
}
