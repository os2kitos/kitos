using AutoFixture;
using Core.DomainModel.ItContract;
using System;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;

namespace Tests.Unit.Presentation.Web.Helpers
{
    public class DataProcessingRegistrationReadModelTestData
    {
        private static readonly Fixture Fixture = new();
        public static DataProcessingRegistrationReadModel CreateReadModel(bool isActive, ItContract mainContract)
        {
            return new DataProcessingRegistrationReadModel
            {
                Id = Fixture.Create<int>(),
                MainContractIsActive = isActive,
                SourceEntity = new DataProcessingRegistration
                {
                    Id = Fixture.Create<int>(),
                    MainContract = mainContract
                }
            };
        }
    }
}
