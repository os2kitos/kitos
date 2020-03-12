using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Interface
{
    public interface IItInterfaceService
    {
        Result<ItInterface, OperationFailure> Delete(int id);
        Result<ItInterface, OperationFailure> Create(ItInterface newInterface);
        Result<ItInterface, OperationFailure> ChangeExposingSystem(int interfaceId, int? newSystemId);
    }
}
