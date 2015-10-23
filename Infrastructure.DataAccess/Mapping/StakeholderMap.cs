using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class StakeholderMap : EntityMap<Stakeholder>
    {
        public StakeholderMap()
        {
            // Properties
            // Table & Column Mappings
            this.ToTable("Stakeholder");
            this.Property(t => t.ItProjectId).HasColumnName("ItProjectId");
        }
    }
}
