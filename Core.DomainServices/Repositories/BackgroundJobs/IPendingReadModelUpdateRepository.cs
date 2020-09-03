using System.Linq;
using Core.DomainModel.BackgroundJobs;

namespace Core.DomainServices.Repositories.BackgroundJobs
{
    public interface IPendingReadModelUpdateRepository
    {
        void AddIfNotPresent(PendingReadModelUpdate newItem);
        void Delete(PendingReadModelUpdate pendingUpdate);
        IQueryable<PendingReadModelUpdate> GetMany(PendingReadModelUpdateSourceCategory category, int maxItems);
    }
}
