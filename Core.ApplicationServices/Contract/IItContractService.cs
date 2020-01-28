using Core.DomainModel.ItContract;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Contract
{
    public interface IItContractService
    {
        Result<ItContract, OperationFailure> Delete(int id);
    }
}
