using System;
using System.Linq;
using Core.DomainModel.KLE;

namespace Core.DomainServices.Repositories.KLE
{
    public interface IKLEStandardRepository
    {
        KLEStatus GetKLEStatus(DateTime lastUpdated);
        IOrderedEnumerable<KLEChange> GetKLEChangeSummary();
        DateTime UpdateKLE(int ownerObjectId, int ownedByOrgnizationUnitId);
    }
}