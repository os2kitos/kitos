using Infrastructure.Services.Http;
using System.Collections.Generic;
using System.Security.Claims;
using Core.ApplicationServices.Authentication;
using Core.DomainModel;
using Core.DomainServices;
using Microsoft.AspNetCore.Http;
using Moq;
using Presentation.Web.Infrastructure.Factories.Authentication;
using Serilog;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Presentation.Web.Context
{
    public class OwinAuthenticationContextFactoryTest : WithAutoFixture
    {
        private readonly int _validUserId;

        public OwinAuthenticationContextFactoryTest()
        {
            _validUserId = A<int>();
        }

        [Fact]
        public void Unauthenticated_User_Should_Return_AuthenticationContext_With_Anonymous_AuthenticationMethod()
        {
            //Arrange
            var authenticationContextFactory = new OwinAuthenticationContextFactory(
                Mock.Of<ILogger>(),
                MakeHttpContextAccessor(authType: null, userId: "1", isAuthenticated: false),
                Mock.Of<IUserRepository>());

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Equal(AuthenticationMethod.Anonymous, authContext.Method);
        }

        [Theory]
        [InlineData(AuthenticationMethod.KitosToken, "JWT")]
        [InlineData(AuthenticationMethod.KitosToken, AuthenticationSchemes.Bearer)]
        [InlineData(AuthenticationMethod.Forms, "Cookies")]
        public void Authenticated_User_Should_Return_AuthenticationContext_With_AuthenticationMethod(AuthenticationMethod authMethod, string authType)
        {
            //Arrange
            var accessor = MakeHttpContextAccessor(authType: authType, userId: _validUserId.ToString(), isAuthenticated: true);
            var userRepository = MakeMockUserRepository(false, _validUserId);
            var authenticationContextFactory = new OwinAuthenticationContextFactory(Mock.Of<ILogger>(), accessor, userRepository);

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Equal(authMethod, authContext.Method);
            Assert.Equal(_validUserId, authContext.UserId);
        }

        [Fact]
        public void Authenticated_User_With_Unknown_AuthType_Returns_Anonymous()
        {
            //Arrange
            var accessor = MakeHttpContextAccessor(authType: "None", userId: _validUserId.ToString(), isAuthenticated: true);
            var userRepository = MakeMockUserRepository(false, _validUserId);
            var authenticationContextFactory = new OwinAuthenticationContextFactory(Mock.Of<ILogger>(), accessor, userRepository);

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.Equal(AuthenticationMethod.Anonymous, authContext.Method);
        }

        [Fact]
        public void Invalid_UserId_Returns_Null()
        {
            //Arrange
            var accessor = MakeHttpContextAccessor(authType: "JWT", userId: "invalid", isAuthenticated: true);
            var userRepository = MakeMockUserRepository(false, _validUserId);
            var authenticationContextFactory = new OwinAuthenticationContextFactory(Mock.Of<ILogger>(), accessor, userRepository);

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
            var accessor = MakeHttpContextAccessor(authType: "JWT", userId: _validUserId.ToString(), isAuthenticated: true);
            var userRepository = MakeMockUserRepository(apiAccess, _validUserId);
            var authenticationContextFactory = new OwinAuthenticationContextFactory(Mock.Of<ILogger>(), accessor, userRepository);

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
            var authenticationContextFactory = new OwinAuthenticationContextFactory(
                Mock.Of<ILogger>(),
                MakeHttpContextAccessor(authType: null, userId: "1", isAuthenticated: false),
                Mock.Of<IUserRepository>());

            //Act
            var authContext = authenticationContextFactory.Create();

            //Assert
            Assert.False(authContext.HasApiAccess);
        }

        private static IUserRepository MakeMockUserRepository(bool apiAccess, int userId)
        {
            var user = new User { HasApiAccess = apiAccess, Id = userId };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(_ => _.GetById(userId)).Returns(user);
            return userRepo.Object;
        }

        private static IHttpContextAccessor MakeHttpContextAccessor(string? authType, string userId, bool isAuthenticated)
        {
            var accessor = new Mock<IHttpContextAccessor>();

            if (!isAuthenticated)
            {
                accessor.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);
                return accessor.Object;
            }

            var claims = new List<Claim> { new(ClaimTypes.Name, userId) };
            var identity = new ClaimsIdentity(claims, authType);
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(c => c.User).Returns(principal);

            accessor.SetupGet(a => a.HttpContext).Returns(httpContext.Object);
            return accessor.Object;
        }
    }
}
