using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices.System
{
    public interface IItSystemService
    {
        Result<ItSystem, OperationError> CreateNewSystem(int organizationId, string name, Guid? uuid = null);
        Result<ItSystem, OperationError> GetSystem(Guid uuid);
        Result<ItSystem, OperationError> GetSystem(int id);
        SystemDeleteResult Delete(int id, bool breakBindings = false);
        IQueryable<ItSystem> GetAvailableSystems(params IDomainQuery<ItSystem>[] conditions);
        IQueryable<ItSystem> GetAvailableSystems(int organizationId, string optionalNameSearch = null);
        Result<IEnumerable<ItSystem>, OperationError> GetCompleteHierarchyByUuid(Guid systemUuid);
        Result<IEnumerable<ItSystem>, OperationError> GetCompleteHierarchy(int systemId);
        Result<IReadOnlyList<UsingOrganization>, OperationFailure> GetUsingOrganizations(int systemId);
        Result<ItSystem, OperationError> UpdateName(int systemId, string newName);
        Result<ItSystem, OperationError> UpdatePreviousName(int systemId, string newPreviousName);
        Result<ItSystem, OperationError> UpdateDescription(int systemId, string newDescription);
        bool CanChangeNameTo(int organizationId, int systemId, string newName);
        bool CanCreateSystemWithName(int organizationId, string name);
        Result<ItSystem, OperationError> UpdateTaskRefs(int systemId, IEnumerable<int> newTaskRefState);
        Result<ItSystem, OperationError> UpdateExternalUuid(int systemId, Guid? newExternalUuid);
        Result<ItSystem, OperationError> UpdateBusinessType(int systemId, Guid? newBusinessTypeState);
        Result<ItSystem, OperationError> UpdateRightsHolder(int systemId, Guid? newRightsHolderState);
        Result<ItSystem, OperationError> UpdateParentSystem(int systemId, int? newParentSystemState = null);
        Result<ItSystem, OperationError> Activate(int itSystemId);
        Result<ItSystem, OperationError> Deactivate(int systemId);
        Result<ItSystem, OperationError> UpdateAccessModifier(int itSystemId, AccessModifier accessModifier);
        Result<SystemPermissions, OperationError> GetPermissions(Guid uuid);
        Result<ResourceCollectionPermissionsResult, OperationError> GetCollectionPermissions(Guid organizationUuid);
    }
}