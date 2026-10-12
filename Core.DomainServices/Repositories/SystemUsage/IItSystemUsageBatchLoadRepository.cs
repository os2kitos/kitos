using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.Repositories.SystemUsage
{
    public interface IItSystemUsageBatchLoadRepository
    {
        IReadOnlyDictionary<int, ItSystemUsage> GetForReadModelRebuild(IEnumerable<int> sourceIds);
    }
}
