using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;

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

        public ItSystemUsageOverviewReadModel Add(ItSystemUsageOverviewReadModel newModel)
        {
            var existing = GetBySourceId(newModel.SourceEntityId);

            if (existing.HasValue)
                throw new InvalidOperationException("Only one read model per entity is allowed");

            var inserted = _repository.Insert(newModel);
            _repository.Save();
            return inserted;
        }

        public void DeleteBySourceId(int sourceId)
        {
            var readModel = GetBySourceId(sourceId);
            if (readModel.HasValue)
            {
                Delete(readModel.Value);
            }
        }

        public void Delete(ItSystemUsageOverviewReadModel readModel)
        {
            if (readModel == null) throw new ArgumentNullException(nameof(readModel));

            _repository.DeleteWithReferencePreload(readModel);
            _repository.Save();
        }

        public Maybe<ItSystemUsageOverviewReadModel> GetBySourceId(int sourceId)
        {
            return _repository.AsQueryable().FirstOrDefault(x => x.SourceEntityId == sourceId);
        }

        public void Update(ItSystemUsageOverviewReadModel readModel)
        {
            _repository.Save();
        }
    }
}
