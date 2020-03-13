using Core.DomainModel.Qa.References;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Qa
{
    public interface IBrokenExternalReferencesReportService
    {
        Result<BrokenExternalReferencesReport, OperationError> GetLatestReport();
    }
}
