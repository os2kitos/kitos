using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.Repositories.SystemUsage
{
    public interface IItSystemUsageRepository
    {
        void Update(ItSystemUsage systemUsage);

        ItSystemUsage GetSystemUsage(int systemUsageId);

        IQueryable<ItSystemUsage> GetSystemUsagesFromOrganization(int organizationId);
        IQueryable<ItSystemUsage> GetBySystemId(int systemId);
        IQueryable<ItSystemUsage> GetByParentSystemId(int parentSystemId);
        IQueryable<ItSystemUsage> GetBySystemIds(IEnumerable<int> systemIds);
        IQueryable<ItSystemUsage> GetByDataProcessingAgreement(int dprId);
    }
}
