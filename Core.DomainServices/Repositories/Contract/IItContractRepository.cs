using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract;


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
        ItContract Add(ItContract itContract);
    }
}
