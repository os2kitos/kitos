using System.Collections.Generic;

namespace Core.DomainServices.Repositories.KLE
{
    public interface IKLEStandardRepository
    {
        KLEStatus GetKLEStatus();
        IReadOnlyList<KLEChange> GetKLEChangeSummary();
        void UpdateKLE(int ownerObjectId, int ownedByOrgnizationUnitId);
    }
}