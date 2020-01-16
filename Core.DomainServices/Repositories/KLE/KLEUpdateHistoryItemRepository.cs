using System;
using System.Collections.Generic;
using System.Data;
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

        public KLEUpdateHistoryItem Insert(DateTime version, int userId)
        {
            KLEUpdateHistoryItem result;
            using (var transaction = _transactionManager.Begin(IsolationLevel.Serializable))
            {
                result = _updateHistoryItems.Insert(new KLEUpdateHistoryItem(version, userId));
                _updateHistoryItems.Save();
                transaction.Commit();
            }
            return result;
        }

        public DateTime GetLastUpdated()
        {
            var lastUpdated = DateTime.MinValue;
            if (_updateHistoryItems.Count > 0)
            {
                lastUpdated = _updateHistoryItems.GetMax(item => item.Version);
            }
            return lastUpdated;
        }
    }
}