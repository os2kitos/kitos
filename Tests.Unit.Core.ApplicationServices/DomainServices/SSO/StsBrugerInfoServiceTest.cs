using Core.DomainServices.SSO;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SSO
{
    public class StsBrugerInfoServiceTest
    {
        //TODO: Must go away before merge
        [Fact]
        private void GetSTSBrugerEmails_GivenValidUuid_ReturnsUserEmail()
        {
            var sut = new StsBrugerInfoService(new StsOrganisationIntegrationConfiguration("1793d097f45b0acea258f7fe18d5a4155799da26", "exttest.serviceplatformen.dk", "58271713") );
            var result = sut.GetStsBrugerInfo("77edccca-4b0d-4dc0-9366-07236e49e965");
            Assert.Contains("neno@balk.dk", result.Emails);
            Assert.Equal("58271713", result.MunicipalityCvr);
            Assert.Equal("", result.BelongsToOrganizationUuid);
        }
    }
}
