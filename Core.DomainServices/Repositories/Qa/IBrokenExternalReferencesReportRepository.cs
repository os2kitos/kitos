using Core.Abstractions.Types;
using Core.DomainModel.Qa.References;


namespace Core.DomainServices.Repositories.Qa
{
    public interface IBrokenExternalReferencesReportRepository
    {
        Maybe<BrokenExternalReferencesReport> LoadCurrentReport();
        void ReplaceCurrentReport(BrokenExternalReferencesReport report);
    }
}
