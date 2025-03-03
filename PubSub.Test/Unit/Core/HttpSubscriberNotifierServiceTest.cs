using PubSub.Core.Services.Notifier;
using Moq;
using Moq.Protected;
using System.Net;
using PubSub.Test.Base.Tests.Toolkit.Patterns;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace PubSub.Test.Unit.Core
{
    public class HttpSubscriberNotifierServiceTest: WithAutoFixture
    {
        [Fact]
        public async Task Can_Post_With_Client_From_Factory()
        {  
            HttpRequestMessage? capturedRequest = null;
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
                {
                    capturedRequest = req;
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                });
            var httpClientFactoryMock = SetupHttpClientFactory(handlerMock.Object);
            var sut = new HttpSubscriberNotifierService(httpClientFactoryMock.Object);
            var message = A<string>();
            var callback = A<string>();

            await sut.Notify(message, callback);

            httpClientFactoryMock.Verify(_ => _.CreateClient(It.IsAny<string>()), Times.Once);
            await AssertHttpContent(message, capturedRequest);
        }

        private async Task AssertHttpContent(string expected, HttpRequestMessage? request) {
            Assert.NotNull(request);
            Assert.NotNull(request.Content);
            var content = await request.Content.ReadAsStringAsync();
            Assert.Contains(expected, content);
        }

        private Mock<IHttpClientFactory> SetupHttpClientFactory(HttpMessageHandler httpMessageHandler)
        {
            var httpClient = new HttpClient(httpMessageHandler) { BaseAddress = A<Uri>() };
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            return httpClientFactoryMock;
        }
    }
}
