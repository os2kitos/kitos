using System;
using System.Collections.Generic;
using Core.DomainModel.KLE;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.KLE
{
    public interface IKLEUpdateHistoryItemRepository
    {
        IEnumerable<KLEUpdateHistoryItem> Get();
        KLEUpdateHistoryItem Insert(DateTime version);
        Maybe<DateTime> GetLastUpdated();
    }
}