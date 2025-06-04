using Microsoft.AspNetCore.Http;
using Moq;
using PubSub.Application.Services.CurrentUserService;
using PubSub.Test.Base.Tests.Toolkit.Patterns;
using System.Security.Claims;
using System.Security.Principal;

namespace PubSub.Test.Unit.Application.Services
{
    public class CurrentUserServiceTest: WithAutoFixture
    {
        [Fact]
        public void UserId_Returns_Id_From_Http_Context()
        {            
            var expectedUserId = A<string>();
            var httpContextAccessor = ExpectUserId(expectedUserId);

            var sut = new CurrentUserService(httpContextAccessor);

            var actualUserId = sut.UserId;

            Assert.True(actualUserId.HasValue);
            Assert.Equal(expectedUserId, actualUserId);
        }

        [Fact]
        public void UserId_Returns_No_UserId_If_None_In_Context()
        {
            var httpContextAccessor = ExpectUserId(null);

            var sut = new CurrentUserService(httpContextAccessor);

            var actualUserId = sut.UserId;

            Assert.False(actualUserId.HasValue);
        }

        private IHttpContextAccessor ExpectUserId(string? userId) {
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new Mock<HttpContext>();
            var identity = new Mock<IIdentity>();
            var user = new Mock<ClaimsPrincipal>();

            httpContextAccessor.Setup(_ => _.HttpContext).Returns(httpContext.Object);
            httpContext.Setup(_ => _.User).Returns(user.Object);
            user.Setup(_ => _.Identity).Returns(identity.Object);
            identity.Setup(_ => _.Name).Returns(userId);

            return httpContextAccessor.Object;
        }
    }
}
