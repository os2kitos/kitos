using Moq;
using PubSub.Core.Consumers;
using PubSub.Core.Managers;
using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;
using RabbitMQ.Client;
using PubSub.Core.Models;
using PubSub.Test.Base;

namespace PubSub.Test.Unit.Core
{
    public class RabbitMQConsumerTest: TestClassWithIChannelBase
    {
        [Fact]
        public async Task Can_Consume_On_Channel()
        {
            var mockConnectionManager = new Mock<IConnectionManager>();
            var connection = new Mock<IConnection>();
            var channel = new Mock<IChannel>();
            mockConnectionManager.Setup(_ => _.GetConnectionAsync()).ReturnsAsync(connection.Object);
            connection.Setup(_ => _.CreateChannelAsync(null, default)).ReturnsAsync(channel.Object);
            var messageSerializer = new Mock<IPayloadSerializer>();

            var mockSubscriberNotifierService = new Mock<ISubscriberNotifierService>();
            var topic = A<Topic>();

            var sut = new RabbitMQConsumer(
                mockConnectionManager.Object,
                mockSubscriberNotifierService.Object,
                messageSerializer.Object,
                topic
            );

            await sut.StartListeningAsync();
            AssertChannelDeclaresQueue(channel);
            AssertChannelConsumes(channel);
        }
    }
}
