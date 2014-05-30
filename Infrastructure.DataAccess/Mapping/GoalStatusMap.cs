using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class GoalStatusMap : EntityMap<GoalStatus>
    {
        public GoalStatusMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("GoalStatus");

            // Relationships
            this.HasRequired(t => t.ItProject)
                .WithOptional(t => t.GoalStatus);

        }
    }
}
