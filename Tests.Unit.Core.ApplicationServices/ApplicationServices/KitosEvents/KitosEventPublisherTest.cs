using Moq;
using Xunit;
using Serilog;
using Core.ApplicationServices.Model.KitosEvents;
using Core.ApplicationServices.KitosEvents;
using Tests.Toolkit.Patterns;
using System;
using System.Net;
using System.Net.Http;
using Core.Abstractions.Types;

namespace Tests.Unit.Core.ApplicationServices.KitosEvents
{
    public class KitosEventPublisherTest : WithAutoFixture
    {
        private readonly Mock<IHttpEventPublisher> _httpEventPublisherMock;
        private readonly Mock<IKitosEventMapper> _kitosEventMapperMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly KitosEventPublisherService _sut;

        public KitosEventPublisherTest()
        {
            _httpEventPublisherMock = new Mock<IHttpEventPublisher>();
            _kitosEventMapperMock = new Mock<IKitosEventMapper>();
            _loggerMock = new Mock<ILogger>();
            _sut = new KitosEventPublisherService(_httpEventPublisherMock.Object, _loggerMock.Object, _kitosEventMapperMock.Object);
        }

        [Fact]
        public void Can_Publish_Event()
        {
            var testEvent = new KitosEvent(A<SystemChangeEventBodyModel>(), A<string>());
            var testDto = A<KitosEventDTO>();
            _kitosEventMapperMock
                .Setup(m => m.MapKitosEventToDTO(testEvent))
                .Returns(testDto);
            _httpEventPublisherMock.Setup(x => x.PostEventAsync(testDto))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            _sut.PublishEvent(testEvent);

            _httpEventPublisherMock.Verify(p => p.PostEventAsync(testDto), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void PublishEvent_FailedResultOrHttpResponse_LogsFailure(bool failedResult)
        {
            var testEvent = new KitosEvent(A<SystemUsageDeletedEventBodyModel>(), A<string>());
            var dto = A<KitosEventDTO>();
            _kitosEventMapperMock.Setup(x => x.MapKitosEventToDTO(testEvent)).Returns(dto);
            Result<HttpResponseMessage, OperationError> response = failedResult
                ? new OperationError(OperationFailure.Forbidden)
                : new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            _httpEventPublisherMock.Setup(x => x.PostEventAsync(dto)).ReturnsAsync(response);

            _sut.PublishEvent(testEvent);

            _loggerMock.Verify(x => x.Error(It.IsAny<string>(), dto.Topic, It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void PublishEvent_PublisherThrows_LogsWithoutThrowing()
        {
            var testEvent = new KitosEvent(A<SystemUsageDeletedEventBodyModel>(), A<string>());
            var dto = A<KitosEventDTO>();
            _kitosEventMapperMock.Setup(x => x.MapKitosEventToDTO(testEvent)).Returns(dto);
            _httpEventPublisherMock.Setup(x => x.PostEventAsync(dto)).ThrowsAsync(new HttpRequestException());

            _sut.PublishEvent(testEvent);

            _loggerMock.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), testEvent.Topic), Times.Once);
        }
    }
}
