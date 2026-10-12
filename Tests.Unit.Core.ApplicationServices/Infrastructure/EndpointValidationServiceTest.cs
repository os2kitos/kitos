using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Infrastructure.Services.Http;
using Moq;
using Serilog;
using Xunit;

#pragma warning disable xUnit1030
namespace Tests.Unit.Core.Infrastructure
{
    public class EndpointValidationServiceTest
    {
        public EndpointValidationServiceTest()
        {
            ServiceEndpointConfiguration.ConfigureValidationOfOutgoingConnections();
        }

        [Fact]
        public async Task ValidateAsync_Skips_Urls_With_Ignored_Protocols()
        {
            var configuration = new Mock<IEndpointValidationConfiguration>();
            var sut = new EndpointValidationService(Mock.Of<ILogger>(), configuration.Object);
            List<string> protocols = ["test1", "test2"];
            configuration.SetupGet(c => c.IgnoredProtocols).Returns(protocols);

            foreach (var protocol in protocols)
            {
                var urlWithProtocol = $"{protocol}://example.com";
                var validation = await sut.ValidateAsync(urlWithProtocol).ConfigureAwait(false);
                Assert.True(validation.Success);

                var withUppercase = urlWithProtocol.ToUpper();
                var validationWithUppercase = await sut.ValidateAsync(withUppercase).ConfigureAwait(false);
                Assert.True(validationWithUppercase.Success);
            }
        }

        [Theory]
        [InlineData("https://kitos.dk/should-not-be-here/", false, EndpointValidationErrorType.ErrorResponseCode, HttpStatusCode.NotFound)]
        [InlineData("http://kitos.dk", true, null, null)] //will upgrade to https
        [InlineData("https://kitos.dk", true, null, null)]
        [InlineData("http://google.com", true, null, null)] //will upgrade to https
        [InlineData("https://google.com", true, null, null)]
        [InlineData("htt:/google.com", false, EndpointValidationErrorType.InvalidWebsiteUri, null)]
        [InlineData("https://d724FF4EE-EA34-4941-88C3-D567958976FF.com", false, EndpointValidationErrorType.DnsLookupFailed, null)]
        [InlineData("https://google.com/icannotbefound1337.com", false, EndpointValidationErrorType.ErrorResponseCode, HttpStatusCode.NotFound)]
        public async Task ValidateAsync_Returns(string candidate, bool success, EndpointValidationErrorType? expectedErrorType, HttpStatusCode? expectedStatusCode)
        {
            //Arrange
            var configuration = new Mock<IEndpointValidationConfiguration>();
            var sut = new EndpointValidationService(Mock.Of<ILogger>(), configuration.Object);

            //Act
            var validation = await sut.ValidateAsync(candidate).ConfigureAwait(false);

            //Assert
            Assert.Equal(success, validation.Success);
            if (!success)
            {
                Assert.NotNull(validation.Error);
                Assert.Equal(expectedErrorType.GetValueOrDefault(), validation.Error.ErrorType);
                Assert.Equal(expectedStatusCode, validation.Error.StatusCode);
            }
        }
    }
}
#pragma warning restore xUnit1030
