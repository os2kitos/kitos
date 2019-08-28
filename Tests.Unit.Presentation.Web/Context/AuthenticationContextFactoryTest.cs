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
using Tests.Unit.Presentation.Web.Helpers;
using Xunit;

namespace Tests.Unit.Presentation.Web.Context
{
    public class AuthenticationContextFactoryTest : WithAutoFixture
    {
        private readonly int _validUserId;
        private const string TokenAuth = "JWT";

        public AuthenticationContextFactoryTest()
        {
            _validUserId = A<int>();
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
        [InlineData(AuthenticationMethod.KitosToken, "JWT", 1234)]
        [InlineData(AuthenticationMethod.Forms, "Forms", 1337)]
        [InlineData(AuthenticationMethod.Anonymous, "None", null)]
        public void Authenticated_User_Should_Return_AuthenticationContext_With_AuthenticationMethod(AuthenticationMethod authMethod, string authType, int? defaultOrg)
        {
            //Arrange
            var owinContext = MakeMockContext(authType: authType, defaultOrg: defaultOrg?.ToString() ?? A<string>(), userId: _validUserId.ToString(), isAuthenticated: "true");
            var userRepository = MakeMockUserRepository(false, _validUserId, defaultOrg);
            var authenticationContextFactory = new AuthenticationContextFactory(Mock.Of<ILogger>(), owinContext, userRepository);

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Equal(authMethod, authContext.Method);
            Assert.Equal(defaultOrg, authContext.ActiveOrganizationId);
            Assert.Equal(_validUserId, authContext.UserId);
        }

        [Fact]
        public void Invalid_Organization_Claim_Value_Returns_Null()
        {
            //Arrange
            var owinContext = MakeMockContext(authType: TokenAuth, defaultOrg: "invalid", userId: _validUserId.ToString(), isAuthenticated: "true");
            var userRepository = MakeMockUserRepository(false, _validUserId);
            var authenticationContextFactory = new AuthenticationContextFactory(Mock.Of<ILogger>(), owinContext, userRepository);

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Null(authContext.ActiveOrganizationId);
            Assert.Equal(AuthenticationMethod.KitosToken, authContext.Method);
        }

        [Fact]
        public void Invalid_UserId_Returns_Null()
        {
            //Arrange
            var owinContext = MakeMockContext(authType: TokenAuth, defaultOrg: "1", userId: "invalid", isAuthenticated: "true");
            var userRepository = MakeMockUserRepository(false, _validUserId);
            var authenticationContextFactory = new AuthenticationContextFactory(Mock.Of<ILogger>(), owinContext, userRepository);

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Null(authContext.UserId);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Authenticated_User_Can_Have_Api_Access(bool apiAccess)
        {
            //Arrange
            var owinContext = MakeMockContext(authType: TokenAuth, defaultOrg: "1", userId: _validUserId.ToString(), isAuthenticated: "true");
            var userRepository = MakeMockUserRepository(apiAccess, _validUserId);
            var authenticationContextFactory = new AuthenticationContextFactory(Mock.Of<ILogger>(), owinContext, userRepository);

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Equal(_validUserId, authContext.UserId);
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

        private static IUserRepository MakeMockUserRepository(bool apiAccess, int userId, int? defaultOrgId = null)
        {
            var user = new User();
            user.HasApiAccess = apiAccess;
            user.Id = userId;
            user.DefaultOrganizationId = defaultOrgId;

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(_ => _.GetById(userId)).Returns(user);
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
            var authManager = new Mock<IAuthenticationManager>();
            var context = new Mock<IOwinContext>();
            context.SetupGet(c => c.Authentication).Returns(authManager.Object);
            context.SetupGet(p => p.Authentication.User).Returns(user);
            return context.Object;
        }
    }
}
