using System.Collections.Generic;

namespace Core.DomainServices.Repositories.Organization
{
    public interface IOrganizationUnitRepository
    {
        IEnumerable<int> GetSubTree(int orgKey, int unitKey);
    }
}
