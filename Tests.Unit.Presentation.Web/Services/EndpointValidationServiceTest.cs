using System.Net;
using System.Threading.Tasks;
using Infrastructure.Services.Http;
using Xunit;

namespace Tests.Unit.Presentation.Web.Services
{
    public class EndpointValidationServiceTest
    {
        [Theory]
        [InlineData("https://google.com", true, null, null)]
        [InlineData("htt:/google.com", false, EndpointValidationErrorType.InvalidWebsiteUri, null)]
        [InlineData("https://d724FF4EE-EA34-4941-88C3-D567958976FF.com", false, EndpointValidationErrorType.DnsLookupFailed, null)]
        [InlineData("https://google.com/icannotbefound1337.com", false, EndpointValidationErrorType.ErrorResponse, HttpStatusCode.NotFound)]
        public async Task Validate_Returns(string candidate, bool success, EndpointValidationErrorType? expectedErrorType, HttpStatusCode? expectedStatusCode)
        {
            //Arrange
            var sut = new EndpointValidationService();

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
