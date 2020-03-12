using System.Net;
using System.Threading.Tasks;
using Infrastructure.Services.Http;
using Moq;
using Serilog;
using Xunit;

namespace Tests.Unit.Core.Infrastructure
{
    public class EndpointValidationServiceTest
    {
        public EndpointValidationServiceTest()
        {
            //Configure how htto client handles TLS and cert validation
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
        }

        [Theory]
        [InlineData("http://127.0.0.1/should-not-be-here/", false, EndpointValidationErrorType.ErrorResponseCode, HttpStatusCode.NotFound)]
        [InlineData("http://kitos.dk", true, null, null)] //will upgrade to https
        [InlineData("https://kitos.dk", true, null, null)]
        [InlineData("http://strongminds.dk", true, null, null)] //will upgrade to https
        [InlineData("https://strongminds.dk", true, null, null)]
        [InlineData("https://google.com", true, null, null)]
        [InlineData("htt:/google.com", false, EndpointValidationErrorType.InvalidWebsiteUri, null)]
        [InlineData("https://d724FF4EE-EA34-4941-88C3-D567958976FF.com", false, EndpointValidationErrorType.DnsLookupFailed, null)]
        [InlineData("https://google.com/icannotbefound1337.com", false, EndpointValidationErrorType.ErrorResponseCode, HttpStatusCode.NotFound)]
        public async Task Validate_Returns(string candidate, bool success, EndpointValidationErrorType? expectedErrorType, HttpStatusCode? expectedStatusCode)
        {
            //Arrange
            var sut = new EndpointValidationService(Mock.Of<ILogger>());

            //Act
            var validation = await sut.ValidateAsync(candidate).ConfigureAwait(false);

            //Assert
            Assert.Equal(success, validation.Success);
            if (!success)
            {
                Assert.Equal(expectedErrorType.GetValueOrDefault(), validation.Error.ErrorType);
                Assert.Equal(expectedStatusCode, validation.Error.StatusCode);
            }
        }
    }
}
