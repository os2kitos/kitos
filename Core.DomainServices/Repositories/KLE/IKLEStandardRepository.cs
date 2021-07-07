using System;
using System.Linq;
using Core.DomainModel.KLE;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.KLE
{
    public interface IKLEStandardRepository
    {
        KLEStatus GetKLEStatus(Maybe<DateTime> lastUpdated);
        IOrderedEnumerable<KLEChange> GetKLEChangeSummary();
        DateTime UpdateKLE(int ownedByOrgnizationUnitId);
    }
}