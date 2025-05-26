using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Qa.References;
using Infrastructure.Services.DataAccess;


namespace Core.DomainServices.Repositories.Qa
{
    public class BrokenExternalReferencesReportRepository : IBrokenExternalReferencesReportRepository
    {
        private readonly IGenericRepository<BrokenExternalReferencesReport> _repository;
        private readonly ITransactionManager _transactionManager;

        public BrokenExternalReferencesReportRepository(
            IGenericRepository<BrokenExternalReferencesReport> repository,
            ITransactionManager transactionManager)
        {
            _repository = repository;
            _transactionManager = transactionManager;
        }

        public Maybe<BrokenExternalReferencesReport> LoadCurrentReport()
        {
            return _repository.AsQueryable().FirstOrDefault();
        }

        public void ReplaceCurrentReport(BrokenExternalReferencesReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            using (var transaction = _transactionManager.Begin())
            {
                var existing = _repository.AsQueryable().FirstOrDefault();
                if (existing != null)
                {
                    if (existing.Id == report.Id)
                    {
                        throw new ArgumentException("Incoming report is the same as the existing report");
                    }
                    _repository.DeleteWithReferencePreload(existing);
                }

                _repository.Insert(report);
                _repository.Save();
                transaction.Commit();
            }
        }
    }
}
