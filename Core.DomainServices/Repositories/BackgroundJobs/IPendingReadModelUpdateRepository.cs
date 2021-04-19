using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.BackgroundJobs;

namespace Core.DomainServices.Repositories.BackgroundJobs
{
    public interface IPendingReadModelUpdateRepository
    {
        void Add(PendingReadModelUpdate newItem);
        void AddMany(IEnumerable<PendingReadModelUpdate> newItems);
        void Delete(PendingReadModelUpdate pendingUpdate);
        IQueryable<PendingReadModelUpdate> GetMany(PendingReadModelUpdateSourceCategory category, int maxItems);
    }
}
