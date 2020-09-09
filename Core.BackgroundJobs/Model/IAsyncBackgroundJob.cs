using System.Threading;
using System.Threading.Tasks;
using Core.DomainModel.Result;

namespace Core.BackgroundJobs.Model
{
    public interface IAsyncBackgroundJob
    {
        string Id { get; }
        Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default);
    }
}
