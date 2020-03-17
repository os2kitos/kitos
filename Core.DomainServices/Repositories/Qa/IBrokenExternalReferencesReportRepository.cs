using Core.DomainModel.Qa.References;
using Core.DomainModel.Result;

namespace Core.DomainServices.Repositories.Qa
{
    public interface IBrokenExternalReferencesReportRepository
    {
        Maybe<BrokenExternalReferencesReport> LoadCurrentReport();
        void ReplaceCurrentReport(BrokenExternalReferencesReport report);
    }
}
