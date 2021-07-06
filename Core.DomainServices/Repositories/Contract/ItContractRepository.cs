using System;
using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Extensions;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Contract
{
    public class ItContractRepository : IItContractRepository
    {
        private readonly IGenericRepository<ItContract> _contractRepository;
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly IDomainEvents _domainEvents;

        public ItContractRepository(
            IGenericRepository<ItContract> contractRepository,
            IGenericRepository<ItSystemUsage> systemUsageRepository, 
            IDomainEvents domainEvents)
        {
            _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
            _systemUsageRepository = systemUsageRepository ?? throw new ArgumentNullException(nameof(systemUsageRepository));
            _domainEvents = domainEvents;
        }

        public IQueryable<ItContract> GetBySystemUsageAssociation(int systemUsageId)
        {
            var usage = _systemUsageRepository.GetByKey(systemUsageId);
            if (usage == null)
            {
                throw new ArgumentException("No system usage found", nameof(systemUsageId));
            }

            var idsOfContractsWithDirectAssociations = usage.Contracts.Select(x => x.ItContractId);


            //Join all contract references wrt. the system usage
            var allContractIds =
                idsOfContractsWithDirectAssociations
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

        public IQueryable<ItContract> GetContractsInOrganization(int organizationId)
        {
            return _contractRepository
                .AsQueryable()
                .ByOrganizationId(organizationId);
        }

        public void DeleteContract(ItContract contract)
        {
            _contractRepository.DeleteWithReferencePreload(contract);
            _contractRepository.Save();
        }

        public void Update(ItContract contract)
        {
            _domainEvents.Raise(new EntityUpdatedEvent<ItContract>(contract));
            _contractRepository.Save();
        }

        public Maybe<ItContract> GetContract(Guid uuid)
        {
            return _contractRepository.AsQueryable().ByUuid(uuid).FromNullable();
        }
    }
}
