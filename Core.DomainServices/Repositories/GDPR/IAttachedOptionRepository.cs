using System.Collections.Generic;
using Core.DomainModel;

namespace Core.DomainServices.Repositories.GDPR
{
    public interface IAttachedOptionRepository
    {
        IEnumerable<AttachedOption> GetBySystemUsageId(int systemUsageId);
        void DeleteBySystemUsageId(int systemUsageId);
    }
}
