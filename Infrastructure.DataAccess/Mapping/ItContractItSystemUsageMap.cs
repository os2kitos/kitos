using Core.DomainModel.ItContract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractItSystemUsageMap : IEntityTypeConfiguration<ItContractItSystemUsage>
    {
        public void Configure(EntityTypeBuilder<ItContractItSystemUsage> builder)
        {
            builder.HasKey(x => new { x.ItContractId, x.ItSystemUsageId });
        }
    }
}
