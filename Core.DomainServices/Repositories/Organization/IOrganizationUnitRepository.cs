using System.Collections.Generic;

namespace Core.DomainServices.Repositories.Organization
{
    public interface IOrganizationUnitRepository
    {
        IEnumerable<int> GetIdsOfSubTree(int organizationId, int organizationUnitId);
    }
}
