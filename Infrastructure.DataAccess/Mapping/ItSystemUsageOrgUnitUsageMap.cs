using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystemUsage;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOrgUnitUsageMap : EntityTypeConfiguration<ItSystemUsageOrgUnitUsage>
    {
        public ItSystemUsageOrgUnitUsageMap()
        {
            HasKey(x => new
            {
                x.ItSystemUsageId, x.OrganizationUnitId
            });
        }
    }
}
