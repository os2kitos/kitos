using System.Collections.Generic;
using System.Security.Claims;
using Core.DomainModel;
using Core.DomainServices;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using NSubstitute;
using Presentation.Web.Infrastructure.Factories.Authentication;
using Presentation.Web.Infrastructure.Model.Authentication;
using Serilog;
using Xunit;

namespace Tests.Unit.Presentation.Web.Context
{
    public class AuthenticationContextTest
    {
        private string tokenAuth = "JWT";
        private string formsAuth = "Forms";
        public AuthenticationContextTest()
        {
        }

        [Fact]
        public void Unauthenticated_user_should_return_AuthenticationContext_with_anonymous_authenticationMethod()
        {
            var authenticationContextFactory = new AuthenticationContextFactory(Substitute.For<ILogger>(), MakeMockContext(null, "invalid", "1", "false"), Substitute.For<IUserRepository>());

            var authContext = authenticationContextFactory.Create();

            Assert.Equal(AuthenticationMethod.Anonymous, authContext.Method);
        }

        [Theory]
        [InlineData(AuthenticationMethod.KitosToken, "JWT", 1)]
        [InlineData(AuthenticationMethod.Forms, "Forms", null)]
        [InlineData(AuthenticationMethod.Anonymous, "None", null)]
        public void Authenticated_user_should_return_AuthenticationContext_with_authenticationMethod(AuthenticationMethod authMethod, string authType, int? defaultOrg)
        {
            var authenticationContextFactory = new AuthenticationContextFactory(Substitute.For<ILogger>(), MakeMockContext(authType, "1", "1", "true"), MakeMockUserRepository(false));

            var authContext = authenticationContextFactory.Create();

            Assert.Equal(authMethod, authContext.Method);
            Assert.Equal(defaultOrg, authContext.ActiveOrganizationId);
            Assert.Equal(1, authContext.UserId);
        }

        [Fact]
        public void Invalid_Organization_Claim_value_returns_null()
        {
            var authenticationContextFactory = new AuthenticationContextFactory(Substitute.For<ILogger>(), MakeMockContext(tokenAuth, "invalid", "1", "true"), MakeMockUserRepository(false));

            var authContext = authenticationContextFactory.Create();

            Assert.Null(authContext.ActiveOrganizationId);
            Assert.Equal(1, authContext.UserId);
            Assert.Equal(AuthenticationMethod.KitosToken, authContext.Method);
        }

        [Fact]
        public void Invalid_userId_returns_zero()
        {
            var authenticationContextFactory = new AuthenticationContextFactory(Substitute.For<ILogger>(), MakeMockContext(tokenAuth, "1", "invalid", "true"), MakeMockUserRepository(false));

            var authContext = authenticationContextFactory.Create();

            Assert.Equal(0, authContext.UserId);
            Assert.Equal(1, authContext.ActiveOrganizationId);
            Assert.Equal(AuthenticationMethod.KitosToken, authContext.Method);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, false)]
        public void Authenticated_User_Can_Have_Api_Access(int userId, bool apiAccess)
        {
            var authenticationContextFactory = new AuthenticationContextFactory(Substitute.For<ILogger>(), MakeMockContext(tokenAuth, "1", userId.ToString(), "true"), MakeMockUserRepository(apiAccess));

            var authContext = authenticationContextFactory.Create();

            Assert.Equal(userId, authContext.UserId);
            Assert.Equal(apiAccess, authContext.HasApiAccess);
        }

        [Fact]
        public void Unauthenticated_User_Can_Not_Have_Api_Access()
        {
            var authenticationContextFactory = new AuthenticationContextFactory(Substitute.For<ILogger>(), MakeMockContext(null, "invalid", "1", "false"), Substitute.For<IUserRepository>());

            var authContext = authenticationContextFactory.Create();

            Assert.Equal(false, authContext.HasApiAccess);
        }

        private IUserRepository MakeMockUserRepository(bool apiAccess)
        {
            var user = new Mock<User>();
            user.Object.HasApiAccess = apiAccess;

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(_ => _.GetById(It.IsAny<int>())).Returns(user.Object);
            return userRepo.Object;
        }



        private IOwinContext MakeMockContext(string authType, string defaultOrg, string userId, string isAuthenticated)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(BearerTokenConfig.DefaultOrganizationClaimName, defaultOrg));
            claims.Add(new Claim(ClaimTypes.Name, userId));
            claims.Add(new Claim(ClaimTypes.Authentication, isAuthenticated, ClaimValueTypes.Boolean));
            var identity = new ClaimsIdentity(claims, authType);
            var user = new ClaimsPrincipal(identity);
            var test = user.Identity.IsAuthenticated;
            var authManager = new Mock<IAuthenticationManager>();
            var context = new Mock<IOwinContext>();
            context.SetupGet(c => c.Authentication).Returns(authManager.Object);
            context.SetupGet(p => p.Authentication.User).Returns(user);
            return context.Object;
        }


    }
}
