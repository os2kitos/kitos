using System.Linq;
using Core.DomainModel.ItContract;

namespace Core.DomainServices.Repositories.Contract
{
    public interface IItContractRepository
    {
        ItContract GetById(int contractId);
        IQueryable<ItContract> GetByOrganizationId(int organizationId);
    }
}
