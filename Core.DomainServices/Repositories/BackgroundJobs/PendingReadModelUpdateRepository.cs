using System;
using System.Data;
using System.Linq;
using Core.DomainModel.BackgroundJobs;
using Infrastructure.Services.DataAccess;

namespace Core.DomainServices.Repositories.BackgroundJobs
{
    public class PendingReadModelUpdateRepository : IPendingReadModelUpdateRepository
    {
        private readonly IGenericRepository<PendingReadModelUpdate> _repository;
        private readonly ITransactionManager _transactionManager;

        public PendingReadModelUpdateRepository(IGenericRepository<PendingReadModelUpdate> repository, ITransactionManager transactionManager)
        {
            _repository = repository;
            _transactionManager = transactionManager;
        }

        public void AddIfNotPresent(PendingReadModelUpdate newItem)
        {
            if (newItem == null)
                throw new ArgumentNullException(nameof(newItem));

            using var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted);

            //Ignore if existing update for same source object within the same update category
            var alreadyPresent = _repository
                .AsQueryable()
                .Where(x => x.SourceId == newItem.SourceId && x.Category == newItem.Category)
                .Any();

            if (!alreadyPresent)
            {
                _repository.Insert(newItem);
                _repository.Save();
                transaction.Commit();
            }
        }

        public void Delete(PendingReadModelUpdate pendingUpdate)
        {
            if (pendingUpdate == null)
                throw new ArgumentNullException(nameof(pendingUpdate));

            _repository.Delete(pendingUpdate);
        }

        public IQueryable<PendingReadModelUpdate> GetMany(PendingReadModelUpdateSourceCategory category, int maxItems)
        {
            return _repository
                .AsQueryable()
                .Where(x => x.Category == category)
                .OrderBy(x => x.Id)
                .Take(maxItems);
        }
    }
}
