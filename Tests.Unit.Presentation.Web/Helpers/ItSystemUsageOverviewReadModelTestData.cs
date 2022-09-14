using System;
using AutoFixture;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;

namespace Tests.Unit.Presentation.Web.Helpers
{
    public static class ItSystemUsageOverviewReadModelTestData
    {
        private static readonly Fixture Fixture = new();
        public static ItSystemUsageOverviewReadModel CreateReadModel(bool isActive, DateTime? sourceConcluded, DateTime? sourceExpirationDate)
        {
            return new ItSystemUsageOverviewReadModel
            {
                Id = Fixture.Create<int>(),
                ActiveAccordingToValidityPeriod = isActive,
                SourceEntity = new ItSystemUsage
                {
                    Id = Fixture.Create<int>(),
                    Concluded = sourceConcluded,
                    ExpirationDate = sourceExpirationDate
                }
            };
        }
    }
}
