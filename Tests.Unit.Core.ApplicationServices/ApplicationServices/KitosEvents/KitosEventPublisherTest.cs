using System;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Serilog;
using Core.ApplicationServices.Model.KitosEvents;
using Core.ApplicationServices.KitosEvents;
using Tests.Toolkit.Patterns;

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

            _sut.PublishEvent(testEvent);

            _httpEventPublisherMock.Verify(p => p.PostEventAsync(testDto), Times.Once);
        }
    }
}
