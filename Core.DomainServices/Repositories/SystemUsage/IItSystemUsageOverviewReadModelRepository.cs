using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;

namespace Core.DomainServices.Repositories.SystemUsage
{
    public interface IItSystemUsageOverviewReadModelRepository
    {
        IQueryable<ItSystemUsageOverviewReadModel> GetByOrganizationId(int organizationId);
    }
}
