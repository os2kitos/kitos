using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Extensions;
using NotImplementedException = System.NotImplementedException;

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

        public ItSystemUsage GetSystemUsage(int systemUsageId)
        {
            return _itSystemUsageRepository.AsQueryable().ById(systemUsageId);
        }

        public IQueryable<ItSystemUsage> GetSystemUsagesFromOrganization(int organizationId)
        {
            return _itSystemUsageRepository.AsQueryable().ByOrganizationId(organizationId);
        }

        public IQueryable<ItSystemUsage> GetBySystemId(int systemId)
        {
            return _itSystemUsageRepository.AsQueryable().Where(x => x.ItSystemId == systemId);
        }

        public IQueryable<ItSystemUsage> GetByParentSystemId(int systemId)
        {
            return _itSystemUsageRepository.AsQueryable().Where(x => x.ItSystem.ParentId == systemId);
        }

        public IQueryable<ItSystemUsage> GetBySystemIdInSystemRelations(int systemId)
        {
            var allRelationsQuery = _itSystemUsageRepository
                .AsQueryable()
                .SelectMany(x => x.UsedByRelations.Concat(x.UsageRelations));

            var allFromSystemUsages = allRelationsQuery
                .Where(x => x.FromSystemUsage.ItSystemId == systemId)
                .Select(x => x.FromSystemUsage);

            var allToSystemUsages = allRelationsQuery
                .Where(x => x.ToSystemUsage.ItSystemId == systemId)
                .Select(x => x.ToSystemUsage);

            return allFromSystemUsages.Concat(allToSystemUsages).Distinct();
        }

        public IQueryable<ItSystemUsage> GetBySystemIds(IEnumerable<int> systemIds)
        {
            var ids = systemIds.ToList();
            return _itSystemUsageRepository.AsQueryable().Where(x => ids.Contains(x.ItSystemId));
        }

        public IQueryable<ItSystemUsage> GetByDataProcessingAgreement(int dprId)
        {
            return _itSystemUsageRepository
                .AsQueryable()
                .Where(x => x.AssociatedDataProcessingRegistrations.Select(r => r.Id).Contains(dprId));
        }
    }
}
