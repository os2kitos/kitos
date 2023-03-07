using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Interface;
using Core.ApplicationServices.Authorization;

namespace Core.ApplicationServices.Interface
{
    public interface IItInterfaceService
    {
        Result<ItInterface, OperationFailure> Delete(int id, bool breakBindings = false);
        IQueryable<ItInterface> GetAvailableInterfaces(params IDomainQuery<ItInterface>[] conditions);
        Result<ItInterface, OperationError> GetInterface(Guid uuid);
        Result<ItInterface, OperationError> CreateNewItInterface(int organizationId, string name, string itInterfaceId, Guid? rightsHolderProvidedUuid = null, AccessModifier? accessModifier = null);
        Result<ItInterface, OperationError> UpdateVersion(int id, string newValue);
        Result<ItInterface, OperationError> UpdateDescription(int id, string newValue);
        Result<ItInterface, OperationError> UpdateUrlReference(int id, string newValue);
        Result<ItInterface, OperationError> UpdateExposingSystem(int interfaceId, int? newSystemId);
        Result<ItInterface, OperationError> UpdateNameAndInterfaceId(int id, string name, string interfaceId);
        Result<ItInterface, OperationError> Deactivate(int id);
        Result<ItInterface, OperationError> Activate(int id);
        Result<ItInterface, OperationError> UpdateNote(int id, string newValue);
        Result<ItInterface, OperationError> UpdateAccessModifier(int id, AccessModifier newValue);
        Result<ItInterface, OperationError> UpdateInterfaceType(int id, Guid? interfaceTypeUuid);
        Result<ItInterface, OperationError> ReplaceInterfaceData(int id, IEnumerable<ItInterfaceDataWriteModel> newData);
        Result<ResourcePermissionsResult, OperationError> GetPermissions(Guid uuid);
        Result<ResourceCollectionPermissionsResult, OperationError> GetCollectionPermissions(Guid organizationUuid);
        Result<DataRow, OperationError> AddInterfaceData(int id, ItInterfaceDataWriteModel parameters);
        Result<DataRow, OperationError> UpdateInterfaceData(int id, Guid dataUuid, ItInterfaceDataWriteModel parameters);
        Result<DataRow, OperationError> DeleteInterfaceData(int id, Guid dataUuid);
    }
}
