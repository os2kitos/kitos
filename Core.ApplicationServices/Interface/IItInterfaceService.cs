using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;
using System;
using System.Linq;

namespace Core.ApplicationServices.Interface
{
    public interface IItInterfaceService
    {
        Result<ItInterface, OperationFailure> Delete(int id);
        Result<ItInterface, OperationFailure> Create(int organizationId, string name, string interfaceId, AccessModifier? accessModifier = AccessModifier.Public);
        Result<ItInterface, OperationFailure> ChangeExposingSystem(int interfaceId, int? newSystemId);
        IQueryable<ItInterface> GetAvailableInterfaces(params IDomainQuery<ItInterface>[] conditions);
        Result<ItInterface, OperationError> GetInterface(Guid uuid);
    }
}
