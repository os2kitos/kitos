using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Extensions;

namespace Core.DomainServices.Repositories.SystemUsage
{
    public class ItSystemUsageRepository : IItSystemUsageRepository
    {
        private readonly IGenericRepository<ItSystemUsage> _itSystemUsageRepository;

        public ItSystemUsageRepository(IGenericRepository<ItSystemUsage> itSystemUsageRepository)
        {
            _itSystemUsageRepository = itSystemUsageRepository;
        }

        public void Update(ItSystemUsage systemUsage)
        {
            _itSystemUsageRepository.Update(systemUsage);
            _itSystemUsageRepository.Save();
        }

        public ItSystemUsage GetSystemUsage(int systemId)
        {
            return _itSystemUsageRepository.AsQueryable().ById(systemId);
        }

        public IQueryable<ItSystemUsage> GetSystemUsagesFromOrganization(int organizationId)
        {
            return _itSystemUsageRepository.AsQueryable().ByOrganizationId(organizationId);
        }
    }
}
