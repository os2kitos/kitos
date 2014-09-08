using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOrgUnitUsageMap : EntityTypeConfiguration<ItSystemUsageOrgUnitUsage>
    {
        public ItSystemUsageOrgUnitUsageMap()
        {
            ToTable("itusageorgusage");
            
            HasKey(x => new
            {
                x.ItSystemUsageId, x.OrganizationUnitId
            });
        }
    }
}
