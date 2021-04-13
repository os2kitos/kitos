using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Extensions;

namespace Core.DomainServices.Repositories.SystemUsage
{
    public class ItSystemUsageOverviewReadModelRepository : IItSystemUsageOverviewReadModelRepository
    {
        private readonly IGenericRepository<ItSystemUsageOverviewReadModel> _repository;

        public ItSystemUsageOverviewReadModelRepository(IGenericRepository<ItSystemUsageOverviewReadModel> repository)
        {
            _repository = repository;
        }

        public IQueryable<ItSystemUsageOverviewReadModel> GetByOrganizationId(int organizationId)
        {
            return _repository.AsQueryable().ByOrganizationId(organizationId);
        }
    }
}
