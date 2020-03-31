using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.Repositories.SystemUsage
{
    public interface IItSystemUsageRepository
    {
        void Update(ItSystemUsage systemUsage);

        ItSystemUsage GetSystemUsage(int systemId);

        IEnumerable<ItSystemUsage> GetSystemUsagesFromOrganization(int organizationId);
    }
}
