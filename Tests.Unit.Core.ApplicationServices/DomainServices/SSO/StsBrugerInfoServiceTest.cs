using System;
using Core.DomainServices.SSO;
using Moq;
using Serilog;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SSO
{
    public class StsBrugerInfoServiceTest
    {
        [Fact]
        private void GetSTSBrugerEmails_GivenValidUuid_ReturnsUserEmail()
        {
            var sut = new StsBrugerInfoService(new StsOrganisationIntegrationConfiguration("1793d097f45b0acea258f7fe18d5a4155799da26", "exttest.serviceplatformen.dk", "58271713"),Mock.Of<ILogger>());
            var maybe = sut.GetStsBrugerInfo( new Guid("77edccca-4b0d-4dc0-9366-07236e49e965"));
            Assert.True(maybe.HasValue);

            var result = maybe.Value;
            Assert.Contains("neno@balk.dk", result.Emails);
            Assert.Equal("58271713", result.MunicipalityCvr);
            Assert.Equal(Guid.Parse("4c5c9482-cab6-4a85-8491-88f98e61d161"), result.BelongsToOrganizationUuid);
            Assert.Equal("Niels", result.FirstName);
            Assert.Equal("Erik  Nordberg", result.LastName); // OBS: Not an error -- "STS returns full name as: 'Niels Erik  Nordberg'
        }
    }
}
