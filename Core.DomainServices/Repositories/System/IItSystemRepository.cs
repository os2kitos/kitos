using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystem;
using Core.DomainServices.Model;

namespace Core.DomainServices.Repositories.System
{
    public interface IItSystemRepository
    {
        IQueryable<ItSystem> GetSystems(OrganizationDataQueryParameters parameters);

        IQueryable<ItSystem> GetUnusedSystems(OrganizationDataQueryParameters parameters);

        IQueryable<ItSystem> GetSystemsInUse(int organizationId);

        ItSystem GetSystem(int systemId);

        void DeleteSystem(ItSystem itSystem);
        IQueryable<ItSystem> GetByRightsHolderId(int sourceId);
        IQueryable<ItSystem> GetByTaskRefId(int taskRefId);
    }
}
