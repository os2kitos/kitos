using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.BackgroundJobs;

namespace Core.DomainServices.Repositories.BackgroundJobs
{
    public class PendingReadModelUpdateRepository : IPendingReadModelUpdateRepository
    {
        private readonly IGenericRepository<PendingReadModelUpdate> _repository;

        public PendingReadModelUpdateRepository(IGenericRepository<PendingReadModelUpdate> repository)
        {
            _repository = repository;
        }

        public void Add(PendingReadModelUpdate newItem)
        {
            if (newItem == null)
                throw new ArgumentNullException(nameof(newItem));

            AddMany(new[] { newItem });
        }

        public void AddMany(IEnumerable<PendingReadModelUpdate> newItems)
        {
            foreach (var pendingReadModelUpdate in newItems)
            {
                _repository.Insert(pendingReadModelUpdate);
            }
            _repository.Save();
        }

        public void Delete(PendingReadModelUpdate pendingUpdate)
        {
            if (pendingUpdate == null)
                throw new ArgumentNullException(nameof(pendingUpdate));

            _repository.Delete(pendingUpdate);
            _repository.Save();
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
