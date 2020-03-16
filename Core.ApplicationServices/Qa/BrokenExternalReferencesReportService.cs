using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Authorization.Permissions;
using Core.DomainModel.Qa.References;
using Core.DomainModel.Result;
using Core.DomainServices.Repositories.Qa;
using Infrastructure.Services.BackgroundJobs;

namespace Core.ApplicationServices.Qa
{
    public class BrokenExternalReferencesReportService : IBrokenExternalReferencesReportService
    {
        private readonly IBrokenExternalReferencesReportRepository _repository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IBackgroundJobScheduler _backgroundJobScheduler;

        public BrokenExternalReferencesReportService(
            IBrokenExternalReferencesReportRepository repository,
            IAuthorizationContext authorizationContext,
            IBackgroundJobScheduler backgroundJobScheduler)
        {
            _repository = repository;
            _authorizationContext = authorizationContext;
            _backgroundJobScheduler = backgroundJobScheduler;
        }


        public Result<BrokenExternalReferencesReport, OperationError> GetLatestReport()
        {
            var currentReportResult = _repository.LoadCurrentReport();

            if (currentReportResult.IsNone)
                return new OperationError(OperationFailure.NotFound);

            var report = currentReportResult.Value;
            if (!_authorizationContext.HasPermission(new ViewBrokenExternalReferencesReportPermission(report)))
                return new OperationError(OperationFailure.Forbidden);

            return report;
        }

        public Maybe<OperationError> TriggerReportGeneration()
        {
            if (!_authorizationContext.HasPermission(new TriggerBrokenReferencesReportPermission()))
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            _backgroundJobScheduler.ScheduleLinkCheck();

            return Maybe<OperationError>.None;
        }
    }
}
