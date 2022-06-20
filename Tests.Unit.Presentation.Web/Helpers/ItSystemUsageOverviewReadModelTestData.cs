using System;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;

namespace Tests.Unit.Presentation.Web.Helpers
{
    public static class ItSystemUsageOverviewReadModelTestData
    {
        public static ItSystemUsageOverviewReadModel CreateReadModel(bool isActive, bool sourceIsActive, DateTime? sourceConcluded, DateTime? sourceExpirationDate)
        {
            return new ItSystemUsageOverviewReadModel
            {
                IsActive = isActive,
                SourceEntity = new ItSystemUsage
                {
                    Active = sourceIsActive,
                    Concluded = sourceConcluded,
                    ExpirationDate = sourceExpirationDate
                }
            };
        }
    }
}
