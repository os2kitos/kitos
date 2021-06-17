using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices.System
{
    public interface IItSystemService
    {
        Result<ItSystem, OperationError> CreateNewSystem(int organizationId, string name, Guid? uuid = null);
        Result<ItSystem, OperationError> GetSystem(Guid uuid);
        Result<ItSystem, OperationError> GetSystem(int id);
        SystemDeleteResult Delete(int id);
        IQueryable<ItSystem> GetAvailableSystems(params IDomainQuery<ItSystem>[] conditions);
        IQueryable<ItSystem> GetAvailableSystems(int organizationId, string optionalNameSearch = null);
        IEnumerable<ItSystem> GetHierarchy(int systemId);
        Result<IReadOnlyList<UsingOrganization>, OperationFailure> GetUsingOrganizations(int systemId);
        Result<ItSystem, OperationError> UpdatePreviousName(int systemId, string newValue);
        Result<ItSystem, OperationError> UpdateDescription(int systemId, string newValue);
        bool CanChangeNameTo(int organizationId, int systemId, string newName);
        bool CanCreateSystemWithName(int organizationId, string name);
        Result<ItSystem, OperationError> UpdateMainUrlReference(int systemId, string urlReference);
        Result<ItSystem, OperationError> UpdateTaskRefs(int systemId, ICollection<int> taskRefIds);
        Result<ItSystem, OperationError> UpdateBusinessType(int systemId, Guid? businessTypeUuid);
        Result<ItSystem, OperationError> UpdateRightsHolder(int systemId, Guid rightsHolderUuid);
        Result<ItSystem, OperationError> UpdateParentSystem(int systemId, int? parentSystemId = null);
    }
}