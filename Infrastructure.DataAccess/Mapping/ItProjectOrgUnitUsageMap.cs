using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItProjectOrgUnitUsageMap : EntityTypeConfiguration<ItProjectOrgUnitUsage>
    {
        public ItProjectOrgUnitUsageMap()
        {
            HasKey(x => new
            {
                x.ItProjectId, x.OrganizationUnitId
            });
        }
    }
}
