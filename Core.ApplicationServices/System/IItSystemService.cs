using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.System
{
    public interface IItSystemService
    {
        Result<ItSystem,OperationError> GetSystem(Guid uuid);
        IQueryable<ItSystem> GetAvailableSystems(params IDomainQuery<ItSystem>[] conditions);
        IQueryable<ItSystem> GetAvailableSystems(int organizationId, string optionalNameSearch = null);
        IEnumerable<ItSystem> GetHierarchy(int systemId);
        Result<IReadOnlyList<UsingOrganization>, OperationFailure> GetUsingOrganizations(int systemId);
        SystemDeleteResult Delete(int id);
        Result<ItSystem,OperationError> CreateNewSystem(int organizationId, string name, Maybe<Guid> uuid);
        bool CanChangeNameTo(int organizationId, int systemId, string newName);
        bool CanCreateSystemWithName(int organizationId, string name);
    }
}