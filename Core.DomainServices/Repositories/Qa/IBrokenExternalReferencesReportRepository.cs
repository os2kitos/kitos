using Core.DomainModel.Qa.References;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Qa
{
    public interface IBrokenExternalReferencesReportRepository
    {
        Maybe<BrokenExternalReferencesReport> LoadCurrentReport();
        void ReplaceCurrentReport(BrokenExternalReferencesReport report);
    }
}
