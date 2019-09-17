using System;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Extensions;

namespace Core.DomainServices.Repositories.Contract
{
    public class ItContractRepository : IItContractRepository
    {
        private readonly IGenericRepository<ItContract> _contractRepository;
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;

        public ItContractRepository(
            IGenericRepository<ItContract> contractRepository,
            IGenericRepository<ItSystemUsage> systemUsageRepository)
        {
            _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
            _systemUsageRepository = systemUsageRepository ?? throw new ArgumentNullException(nameof(systemUsageRepository));
        }

        public IQueryable<ItContract> GetBySystemUsageAssociation(int systemUsageId)
        {
            var usage = _systemUsageRepository.GetByKey(systemUsageId);
            if (usage == null)
            {
                throw new ArgumentException("No system usage found", nameof(systemUsageId));
            }

            var idsOfContractsWithDirectAssociations = usage.Contracts.Select(x => x.ItContractId);
            var interfaceExhibitUsages = usage.ItInterfaceExhibitUsages.ToList();
            var interfaceUsages = usage.ItInterfaceUsages.ToList();

            //Join all contract references wrt. the system usage
            var allContractIds =
                idsOfContractsWithDirectAssociations
                    .Concat(
                        interfaceUsages
                            .Select(x => x.ItContractId)
                            .Concat(interfaceExhibitUsages.Select(x => x.ItContractId))
                            .Where(id => id.HasValue)
                            .Select(x => x.Value))
                    .Distinct()
                    .ToList();

            return _contractRepository
                .AsQueryable()
                .ByIds(allContractIds);
        }

        public ItContract GetById(int contractId)
        {
            return _contractRepository.AsQueryable().ById(contractId);
        }
    }
}
