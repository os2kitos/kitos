using System.Collections.Generic;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.KLE;
using Core.DomainServices.Model.Result;
using Core.DomainServices.Repositories.KLE;

namespace Core.ApplicationServices.TaskRefs
{
    public interface IKLEApplicationService
    {
        Result<KLEStatus, OperationFailure> GetKLEStatus();
        Result<IEnumerable<KLEChange>, OperationFailure> GetKLEChangeSummary();
        Result<KLEUpdateStatus, OperationFailure> UpdateKLE();
    }
}