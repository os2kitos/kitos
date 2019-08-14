using System.Collections.Generic;
using System.Security.Claims;
using Core.DomainModel;
using Core.DomainServices;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq;
using Presentation.Web.Infrastructure.Factories.Authentication;
using Presentation.Web.Infrastructure.Model.Authentication;
using Serilog;
using Xunit;

namespace Tests.Unit.Presentation.Web.Context
{
    public class AuthenticationContextFactoryTest
    {
        private string _tokenAuth = "JWT";
        private string _formsAuth = "Forms";
        public AuthenticationContextFactoryTest()
        {
        }

        [Fact]
        public void Unauthenticated_User_Should_Return_AuthenticationContext_With_Anonymous_AuthenticationMethod()
        {
            //Arrange
            var authenticationContextFactory = new AuthenticationContextFactory(Mock.Of<ILogger>(), MakeMockContext(authType: null, defaultOrg: "invalid", userId: "1", isAuthenticated: "false"), Mock.Of<IUserRepository>());

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Equal(AuthenticationMethod.Anonymous, authContext.Method);
        }

        [Theory]
        [InlineData(AuthenticationMethod.KitosToken, "JWT", 1)]
        [InlineData(AuthenticationMethod.Forms, "Forms", null)]
        [InlineData(AuthenticationMethod.Anonymous, "None", null)]
        public void Authenticated_User_Should_Return_AuthenticationContext_With_AuthenticationMethod(AuthenticationMethod authMethod, string authType, int? defaultOrg)
        {
            //Arrange
            var authenticationContextFactory = new AuthenticationContextFactory(Mock.Of<ILogger>(), MakeMockContext(authType: authType, defaultOrg: "1", userId: "1", isAuthenticated: "true"), MakeMockUserRepository(false));

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Equal(authMethod, authContext.Method);
            Assert.Equal(defaultOrg, authContext.ActiveOrganizationId);
            Assert.Equal(1, authContext.UserId);
        }

        [Fact]
        public void Invalid_Organization_Claim_Value_Returns_Null()
        {
            //Arrange
            var authenticationContextFactory = new AuthenticationContextFactory(Mock.Of<ILogger>(), MakeMockContext(authType: _tokenAuth, defaultOrg: "invalid", userId: "1", isAuthenticated: "true"), MakeMockUserRepository(false));

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Null(authContext.ActiveOrganizationId);
            Assert.Equal(1, authContext.UserId);
            Assert.Equal(AuthenticationMethod.KitosToken, authContext.Method);
        }

        [Fact]
        public void Invalid_UserId_Returns_Null()
        {
            //Arrange
            var authenticationContextFactory = new AuthenticationContextFactory(Mock.Of<ILogger>(), MakeMockContext(authType: _tokenAuth, defaultOrg: "1", userId: "invalid", isAuthenticated: "true"), MakeMockUserRepository(false));

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Null(authContext.UserId);
            Assert.Equal(1, authContext.ActiveOrganizationId);
            Assert.Equal(AuthenticationMethod.KitosToken, authContext.Method);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(2, false)]
        public void Authenticated_User_Can_Have_Api_Access(int userId, bool apiAccess)
        {
            //Arrange
            var authenticationContextFactory = new AuthenticationContextFactory(Mock.Of<ILogger>(), MakeMockContext(authType: _tokenAuth, defaultOrg: "1", userId: userId.ToString(), isAuthenticated: "true"), MakeMockUserRepository(apiAccess));

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Equal(userId, authContext.UserId);
            Assert.Equal(apiAccess, authContext.HasApiAccess);
        }

        [Fact]
        public void Unauthenticated_User_Can_Not_Have_Api_Access()
        {
            //Arrange
            var authenticationContextFactory = new AuthenticationContextFactory(Mock.Of<ILogger>(), MakeMockContext(authType: null, defaultOrg: "invalid", userId: "1", isAuthenticated: "false"), Mock.Of<IUserRepository>());

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
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
            var claims = new List<Claim>
            {
                new Claim(BearerTokenConfig.DefaultOrganizationClaimName, defaultOrg),
                new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.Authentication, isAuthenticated, ClaimValueTypes.Boolean)
            };
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
