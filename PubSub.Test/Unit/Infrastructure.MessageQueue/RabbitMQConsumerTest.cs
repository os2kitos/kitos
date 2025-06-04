using Moq;
using RabbitMQ.Client;
using PubSub.Test.Base;
using PubSub.Infrastructure.MessageQueue.Consumer;
using PubSub.Core.DomainModel.Serializer;
using PubSub.Application.Services.RabbitMQUtils;
using PubSub.Core.DomainModel.Notifier;
using Microsoft.Extensions.DependencyInjection;

namespace PubSub.Test.Unit.Infrastructure.MessageQueue
{
    public class RabbitMQConsumerTest : TestClassWithIChannelBase
    {
        [Fact]
        public async Task Can_Consume_On_Channel()
        {
            var mockConnectionManager = new Mock<IRabbitMQConnectionManager>();
            var connection = new Mock<IConnection>();
            var channel = new Mock<IChannel>();
            var repo = new Mock<IServiceScopeFactory>();
            mockConnectionManager.Setup(_ => _.GetConnectionAsync()).ReturnsAsync(connection.Object);
            connection.Setup(_ => _.CreateChannelAsync(null, default)).ReturnsAsync(channel.Object);
            var messageSerializer = new Mock<IJsonPayloadSerializer>();

            var mockSubscriberNotifierService = new Mock<ISubscriberNotifier>();
            var topic = A<string>();

            var sut = new RabbitMQConsumer(
                mockConnectionManager.Object,
                mockSubscriberNotifierService.Object,
                messageSerializer.Object,
                topic,
                repo.Object
            );

            await sut.StartListeningAsync();
            AssertChannelDeclaresQueue(channel);
            AssertChannelConsumes(channel);
        }
    }
}
