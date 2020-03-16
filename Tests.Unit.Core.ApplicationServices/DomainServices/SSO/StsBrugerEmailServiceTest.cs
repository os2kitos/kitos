using Core.DomainServices.SSO;
using Xunit;

namespace Tests.Unit.Core.DomainServices.SSO
{
    public class StsBrugerEmailServiceTest
    {
        [Fact]
        private void GetSTSBrugerEmails_GivenValidUuid_ReturnsUserEmail()
        {
            var sut = new StsBrugerEmailService();
            // TODO: Move guid + email to AWS Parameter Store
            var result = sut.GetStsBrugerEmails("77edccca-4b0d-4dc0-9366-07236e49e965");
            Assert.Contains("neno@balk.dk", result);
        }
    }
}
