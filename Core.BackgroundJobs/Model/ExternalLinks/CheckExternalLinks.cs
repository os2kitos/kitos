using System.Threading.Tasks;
using Core.DomainModel.Result;
using Serilog;

namespace Core.BackgroundJobs.Model.ExternalLinks
{
    public class CheckExternalLinks : IAsyncBackgroundJob
    {
        private readonly ILogger _logger;
        public string Id => StandardJobIds.CheckExternalLinks;

        public CheckExternalLinks(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<Result<string, OperationError>> ExecuteAsync()
        {
            _logger.Debug("Executed background job:" + nameof(CheckExternalLinks));
            return "Success";
        }
    }
}
