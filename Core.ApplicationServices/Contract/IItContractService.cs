using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Contract
{
    public interface IItContractService
    {
        IQueryable<ItContract> GetAllByOrganization(int orgId, string optionalNameSearch = null);
        Result<ItContract, OperationFailure> Delete(int id);
    }
}
