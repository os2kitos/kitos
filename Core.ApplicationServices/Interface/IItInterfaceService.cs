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
        IQueryable<ItInterface> GetAvailableInterfaces(params IDomainQuery<ItInterface>[] conditions);
        Result<ItInterface, OperationError> GetInterface(Guid uuid);
        Result<ItInterface, OperationError> CreateNewItInterface(int organizationId, string name, string itInterfaceId, Guid? rightsHolderProvidedUuid = null, AccessModifier? accessModifier = null);
        Result<ItInterface, OperationError> UpdateVersion(int id, string newValue);
        Result<ItInterface, OperationError> UpdateDescription(int id, string newValue);
        Result<ItInterface, OperationError> UpdateUrlReference(int id, string newValue);
        Result<ItInterface, OperationError> UpdateExposingSystem(int interfaceId, int? newSystemId);
        Result<ItInterface, OperationError> UpdateNameAndInterfaceId(int id, string name, string interfaceId);
    }
}
