using System;
using System.Collections.Generic;
using Core.DomainModel.KLE;
using Core.DomainServices.Model.Result;

namespace Core.DomainServices.Repositories.KLE
{
    public interface IKLEUpdateHistoryItemRepository
    {
        IEnumerable<KLEUpdateHistoryItem> Get();
        KLEUpdateHistoryItem Insert(DateTime version, int userId);
        DateTime GetLastUpdated();
    }
}