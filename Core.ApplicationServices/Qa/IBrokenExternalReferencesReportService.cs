using Core.DomainModel.Qa.References;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Qa
{
    public interface IBrokenExternalReferencesReportService
    {
        Result<BrokenExternalReferencesReport, OperationError> GetLatestReport();
        Maybe<OperationError> TriggerReportGeneration();
    }
}
