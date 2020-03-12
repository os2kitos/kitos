using Core.ApplicationServices.SSO;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.SSO
{
    public class SSOFlowApplicationServiceTest
    {
        [Fact]
        private void GetSTSBrugerInfo_WhenFails_ThrowsSTSException()
        {
            var sut = new SSOFlowApplicationService();
            var result = sut.GetStsBrugerEmail("77edccca-4b0d-4dc0-9366-07236e49e965");
            Assert.Equal("neno@balk.dk", result);
        }
    }
}
