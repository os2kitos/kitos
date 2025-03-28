using PubSub.Core.Services.Notifier;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using PubSub.Test.Base.Tests.Toolkit.Patterns;
using PubSub.Core.Services.CallbackAuthentication;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PubSub.Test.Unit.Core
{
    public class HttpSubscriberNotifierServiceTest: WithAutoFixture
    {
        private class DummyDTO
        {
            public string Field { get; set; }
        }
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
            var callbackAuthenticator = new Mock<ICallbackAuthenticator>();
            var sut = new HttpSubscriberNotifierService(httpClientFactoryMock.Object, callbackAuthenticator.Object);
           
            var callback = A<string>();
            var dummy = new DummyDTO { Field = "TestValue" };
            var json = JsonSerializer.Serialize(dummy);
            var payload = JsonDocument.Parse(json).RootElement;

            await sut.Notify(payload, callback);

            httpClientFactoryMock.Verify(_ => _.CreateClient(It.IsAny<string>()), Times.Once);
            await AssertHttpContent(payload, capturedRequest);
        }

        private async Task AssertHttpContent(JsonElement expected, HttpRequestMessage? request) {
            Assert.NotNull(request);
            Assert.NotNull(request.Content);
            var content = await request.Content.ReadAsStringAsync();
            Assert.Contains(expected.GetRawText(), content);
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
