using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItContractItSystemUsageMap : EntityTypeConfiguration<ItContractItSystemUsage>
    {
        public ItContractItSystemUsageMap()
        {
            HasKey(x => new
            {
                x.ItContractId, x.ItSystemUsageId
            });
        }
    }
}
