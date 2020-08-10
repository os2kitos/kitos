using System.Collections.Generic;

namespace Core.DomainModel
{
    public interface IIsPartOfOrganization
    {
        IEnumerable<int> GetOrganizationIds();
    }
}
