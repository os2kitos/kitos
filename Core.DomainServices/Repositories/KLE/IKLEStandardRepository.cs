using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.KLE;


namespace Core.DomainServices.Repositories.KLE
{
    public interface IKLEStandardRepository
    {
        KLEStatus GetKLEStatus(Maybe<DateTime> lastUpdated);
        IOrderedEnumerable<KLEChange> GetKLEChangeSummary();
        DateTime UpdateKLE(int ownedByOrgnizationUnitId);
    }
}