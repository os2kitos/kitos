using System.Collections.Generic;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.System;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Core.ApplicationServices.ItSystemServices
{
    public interface IItSystemService
    {
        IEnumerable<ItSystem> GetSystems(int organizationId, string nameSearch, User user);
        IEnumerable<ItSystem> GetNonInterfaces(int organizationId, string nameSearch, User user);
        IEnumerable<ItSystem> GetInterfaces(int organizationId, string nameSearch, User user);
        IEnumerable<ItSystem> GetHierarchy(int systemId);
        Result<OperationResult, IReadOnlyList<UsingOrganization>> GetUsingOrganizations(int systemId);
        void Delete(int id);
    }
}