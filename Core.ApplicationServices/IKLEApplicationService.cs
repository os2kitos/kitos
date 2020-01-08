using Core.ApplicationServices.Model.Result;
using Core.DomainServices.Repositories.KLE;

namespace Core.ApplicationServices
{
    public interface IKLEApplicationService
    {
        Result<OperationResult, KLEStatus> GetKLEStatus();
    }
}