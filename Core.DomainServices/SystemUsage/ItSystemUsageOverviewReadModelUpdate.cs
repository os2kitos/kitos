using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Model;

namespace Core.DomainServices.SystemUsage
{
    public class ItSystemUsageOverviewReadModelUpdate : IReadModelUpdate<DomainModel.ItSystemUsage.ItSystemUsage, ItSystemUsageOverviewReadModel>
    {
        public void Apply(DomainModel.ItSystemUsage.ItSystemUsage source, ItSystemUsageOverviewReadModel destination)
        {
            destination.Name = source.ItSystem.Name;
        }
    }
}
