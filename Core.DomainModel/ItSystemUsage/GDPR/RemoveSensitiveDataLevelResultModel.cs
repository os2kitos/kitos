using System.Collections.Generic;

namespace Core.DomainModel.ItSystemUsage.GDPR
{
    public class RemoveSensitiveDataLevelResultModel
    {
        public RemoveSensitiveDataLevelResultModel(ItSystemUsageSensitiveDataLevel removedRiskLevel, IEnumerable<ItSystemUsagePersonalData> removedPersonalDataOptions)
        {
            RemovedRiskLevel = removedRiskLevel;
            RemovedPersonalDataOptions = removedPersonalDataOptions;
        }

        public ItSystemUsageSensitiveDataLevel RemovedRiskLevel { get; }
        public IEnumerable<ItSystemUsagePersonalData> RemovedPersonalDataOptions{ get; }
    }
}
