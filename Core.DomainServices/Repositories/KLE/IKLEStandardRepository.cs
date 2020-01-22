using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Core.DomainModel.KLE;

namespace Core.DomainServices.Repositories.KLE
{
    public interface IKLEStandardRepository
    {
        KLEStatus GetKLEStatus(DateTime lastUpdated);
        IReadOnlyList<KLEChange> GetKLEChangeSummary();
        DateTime UpdateKLE(int ownerObjectId, int ownedByOrgnizationUnitId);
    }
}