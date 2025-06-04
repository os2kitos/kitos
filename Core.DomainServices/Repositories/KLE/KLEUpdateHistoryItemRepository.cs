using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainModel.KLE;
using Infrastructure.Services.DataAccess;


namespace Core.DomainServices.Repositories.KLE
{
    public class KLEUpdateHistoryItemRepository : IKLEUpdateHistoryItemRepository
    {
        private readonly IGenericRepository<KLEUpdateHistoryItem> _updateHistoryItems;
        private readonly ITransactionManager _transactionManager;

        public KLEUpdateHistoryItemRepository(IGenericRepository<KLEUpdateHistoryItem> updateHistoryItems, ITransactionManager transactionManager)
        {
            _updateHistoryItems = updateHistoryItems;
            _transactionManager = transactionManager;
        }

        public IEnumerable<KLEUpdateHistoryItem> Get()
        {
            return _updateHistoryItems.Get();
        }

        public KLEUpdateHistoryItem Insert(DateTime version)
        {
            using var transaction = _transactionManager.Begin();
            var result = _updateHistoryItems.Insert(new KLEUpdateHistoryItem(version));
            _updateHistoryItems.Save();
            transaction.Commit();
            return result;
        }

        public Maybe<DateTime> GetLastUpdated()
        {
            var lastUpdated = Maybe<DateTime>.None;
            if (_updateHistoryItems.Count > 0)
            {
                lastUpdated = _updateHistoryItems.GetMax(item => item.Version);
            }
            return lastUpdated;
        }
    }
}