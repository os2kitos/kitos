using Core.Abstractions.Types;
using Core.DomainModel.Qa.References;


namespace Core.ApplicationServices.Qa
{
    public interface IBrokenExternalReferencesReportService
    {
        Result<BrokenExternalReferencesReport, OperationError> GetLatestReport();
        Maybe<OperationError> TriggerReportGeneration();
    }
}
