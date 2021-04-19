using System;
using System.Collections.Generic;
using System.Linq;
using Core.BackgroundJobs.Model;
using Core.BackgroundJobs.Model.ReadModels;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Core.DomainServices.Repositories.BackgroundJobs;
using Infrastructure.Services.BackgroundJobs;

namespace Core.BackgroundJobs.Factories
{
    public class RebuildReadModelsJobFactory : IRebuildReadModelsJobFactory
    {
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly IGenericRepository<DataProcessingRegistration> _dprRepository;
        private readonly IPendingReadModelUpdateRepository _pendingReadModelUpdateRepository;

        public RebuildReadModelsJobFactory(
            IGenericRepository<ItSystemUsage> systemUsageRepository,
            IGenericRepository<DataProcessingRegistration> dprRepository,
            IPendingReadModelUpdateRepository pendingReadModelUpdateRepository)
        {
            _systemUsageRepository = systemUsageRepository;
            _dprRepository = dprRepository;
            _pendingReadModelUpdateRepository = pendingReadModelUpdateRepository;
        }

        public IAsyncBackgroundJob CreateRebuildJob(ReadModelRebuildScope scope)
        {
            string id;
            Func<IEnumerable<int>> getIds = null;
            PendingReadModelUpdateSourceCategory updateSourceCategory;
            switch (scope)
            {
                case ReadModelRebuildScope.ItSystemUsage:
                    id = StandardJobIds.RebuildItSystemUsageReadModels;
                    getIds = () => _systemUsageRepository.AsQueryable().Select(x => x.Id).AsEnumerable();
                    updateSourceCategory = PendingReadModelUpdateSourceCategory.ItSystemUsage;
                    break;
                case ReadModelRebuildScope.DataProcessingRegistration:
                    id = StandardJobIds.RebuildDataProcessingReadModels;
                    getIds = () => _dprRepository.AsQueryable().Select(x => x.Id).AsEnumerable();
                    updateSourceCategory = PendingReadModelUpdateSourceCategory.DataProcessingRegistration;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
            return new RebuildReadModelsJob(id, getIds, updateSourceCategory, _pendingReadModelUpdateRepository);
        }
    }
}
