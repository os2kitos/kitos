using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class GoalStatusMap : EntityTypeConfiguration<GoalStatus>
    {
        public GoalStatusMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("GoalStatus");
            this.Property(t => t.Id).HasColumnName("Id");

            // Relationships
            this.HasRequired(t => t.ItProject)
                .WithOptional(t => t.GoalStatus);

        }
    }
}
