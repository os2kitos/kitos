using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions.Types;

namespace Core.BackgroundJobs.Model
{
    public interface IAsyncBackgroundJob
    {
        string Id { get; }
        Task<Result<string, OperationError>> ExecuteAsync(CancellationToken token = default);
    }
}
