using Core.DomainModel.ItSystemUsage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemUsageOrgUnitUsageMap : IEntityTypeConfiguration<ItSystemUsageOrgUnitUsage>
    {
        public void Configure(EntityTypeBuilder<ItSystemUsageOrgUnitUsage> builder)
        {
            builder.HasKey(x => new { x.ItSystemUsageId, x.OrganizationUnitId });
        }
    }
}
