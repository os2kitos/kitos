using Microsoft.Extensions.DependencyInjection;
using Moq;
using PubSub.Application.Services;
using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;
using PubSub.Test.Base.Tests.Toolkit.Patterns;

namespace PubSub.Test.Unit.Core
{
    public class RabbitMQTopicConsumerInstantiatorServiceTest : WithAutoFixture
    {
        private RabbitMQTopicConsumerInstantiatorService _sut;
        private readonly Mock<ITopicConsumerStore> _subscriptionStore;
        private readonly Mock<IRabbitMQConsumerFactory> _consumerFactory;
        private readonly Mock<IConnectionManager> _mockConnectionManager;
        private readonly Mock<ISubscriberNotifierService> _mockSubscriberNotifierService;
        private readonly Mock<IPayloadSerializer> _messageSerializer;
        private readonly Mock<IServiceScopeFactory> _subscriptionRepository;


        public RabbitMQTopicConsumerInstantiatorServiceTest()
        {
            _mockConnectionManager = new Mock<IConnectionManager>();
            _subscriptionStore = new Mock<ITopicConsumerStore>();
            _consumerFactory = new Mock<IRabbitMQConsumerFactory>();
            _mockSubscriberNotifierService = new Mock<ISubscriberNotifierService>();
            _messageSerializer = new Mock<IPayloadSerializer>();
            _subscriptionRepository = new Mock<IServiceScopeFactory>();
            _sut = new RabbitMQTopicConsumerInstantiatorService(_mockConnectionManager.Object, _mockSubscriberNotifierService.Object, _subscriptionStore.Object, _consumerFactory.Object, _messageSerializer.Object, _subscriptionRepository.Object);
        }

        [Fact]
        public async Task Can_Add_And_Start_New_Consumer_If_None_Found_For_Topic()
        {
            var topic = A<string>();
            var consumer = new Mock<IConsumer>();
            _subscriptionStore.Setup(x => x.HasConsumer(topic)).Returns(false);
            _consumerFactory.Setup(x => x.Create(It.IsAny<IConnectionManager>(), It.IsAny<ISubscriberNotifierService>(), It.IsAny<IPayloadSerializer>(), It.IsAny<string>(), It.IsAny<IServiceScopeFactory>())).Returns(consumer.Object);

            await _sut.InstantiateTopic(topic);

            _subscriptionStore.Verify(x => x.SetConsumerForTopic(topic, consumer.Object));
            consumer.Verify(x => x.StartListeningAsync());
        }
    }
}
