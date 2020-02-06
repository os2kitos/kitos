using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.System;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.System
{
    public interface IItSystemService
    {
        IQueryable<ItSystem> GetAvailableSystems(int organizationId, string optionalNameSearch = null);
        IEnumerable<ItSystem> GetHierarchy(int systemId);
        Result<IReadOnlyList<UsingOrganization>, OperationFailure> GetUsingOrganizations(int systemId);
        SystemDeleteResult Delete(int id);
    }
}