using System.Collections.Generic;
using Core.DomainModel.KLE;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.KLE;

namespace Core.ApplicationServices.KLE
{
    public interface IKLEApplicationService
    {
        Result<KLEStatus, OperationFailure> GetKLEStatus();
        Result<IEnumerable<KLEChange>, OperationFailure> GetKLEChangeSummary();
        Result<KLEUpdateStatus, OperationFailure> UpdateKLE(int organizationId);
    }
}