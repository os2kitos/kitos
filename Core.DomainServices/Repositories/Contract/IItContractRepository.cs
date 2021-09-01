using System;
using System.Linq;
using Core.DomainModel.ItContract;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Contract
{
    public interface IItContractRepository
    {
        IQueryable<ItContract> GetBySystemUsageAssociation(int systemUsageId);
        ItContract GetById(int contractId);
        IQueryable<ItContract> GetContractsInOrganization(int organizationId);
        void DeleteContract(ItContract contract);
        void Update(ItContract contract);
        Maybe<ItContract> GetContract(Guid uuid);
    }
}
