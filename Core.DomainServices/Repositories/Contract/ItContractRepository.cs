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

        public ItContract GetById(int contractId)
        {
            return _contractRepository.AsQueryable().ById(contractId);
        }

        public IQueryable<ItContract> GetByOrganizationId(int organizationId)
        {
            return _contractRepository
                .AsQueryable()
                .ByOrganizationId(organizationId);
        }
    }
}
