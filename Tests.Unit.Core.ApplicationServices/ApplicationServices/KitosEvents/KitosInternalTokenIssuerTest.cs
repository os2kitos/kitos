using System;
using Core.ApplicationServices;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Model.Authentication;
using Core.DomainModel;
using Moq;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Unit.Core.ApplicationServices.KitosEvents
{
    public class KitosInternalTokenIssuerTest : WithAutoFixture
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ITokenValidator> _tokenValidatorMock;
        private readonly KitosInternalTokenIssuer _sut;

        public KitosInternalTokenIssuerTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _tokenValidatorMock = new Mock<ITokenValidator>();
            _sut = new KitosInternalTokenIssuer(_userServiceMock.Object, _tokenValidatorMock.Object);
        }

        [Fact]
        public void Can_Issue_Token()
        {
            var expectedUser = new User { IsGlobalAdmin = true };
            _userServiceMock.Setup(x => x.GetGlobalAdmin()).Returns(expectedUser);

            var expectedToken = new KitosApiToken(expectedUser, A<string>(), A<DateTime>());
            _tokenValidatorMock
                .Setup(x => x.CreateToken(expectedUser))
                .Returns(expectedToken);

            var result = _sut.GetToken();

            Assert.True(result.Ok);
            Assert.Equal(expectedToken, result.Value);
        }

    }
}
