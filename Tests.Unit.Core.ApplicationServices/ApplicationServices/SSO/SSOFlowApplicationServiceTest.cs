using System.Linq;
using Core.ApplicationServices.SSO;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.SSO
{
    public class SSOFlowApplicationServiceTest
    {
        [Fact]
        private void GetSTSBrugerEmails_GivenValidUuid_ReturnsUserEmail()
        {
            var sut = new SSOFlowApplicationService();
            var result = sut.GetStsBrugerEmails("77edccca-4b0d-4dc0-9366-07236e49e965");
            Assert.Contains("neno@balk.dk", result);
        }
    }
}
