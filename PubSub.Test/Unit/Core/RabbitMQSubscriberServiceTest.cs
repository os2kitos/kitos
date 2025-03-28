using Moq;
using PubSub.Core.Consumers;
using PubSub.Core.Managers;
using PubSub.Core.Models;
using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Subscribe;
using PubSub.Core.Services.Serializer;
using PubSub.Test.Base.Tests.Toolkit.Patterns;

namespace PubSub.Test.Unit.Core
{
    public class RabbitMQSubscriberServiceTest: WithAutoFixture
    {
        private RabbitMQSubscriberService _sut;
        private readonly Mock<ISubscriptionStore> _subscriptionStore;
        private readonly Mock<IRabbitMQConsumerFactory> _consumerFactory;
        private readonly Mock<IConnectionManager> _mockConnectionManager;
        private readonly Mock<ISubscriberNotifierService> _mockSubscriberNotifierService;
        private readonly Mock<IPayloadSerializer> _messageSerializer;

        public RabbitMQSubscriberServiceTest()
        {
            _mockConnectionManager = new Mock<IConnectionManager>();
            _subscriptionStore = new Mock<ISubscriptionStore>();
            _consumerFactory = new Mock<IRabbitMQConsumerFactory>();
            _mockSubscriberNotifierService = new Mock<ISubscriberNotifierService>();
            _messageSerializer = new Mock<IPayloadSerializer>();
            _sut = new RabbitMQSubscriberService(_mockConnectionManager.Object, _mockSubscriberNotifierService.Object, _subscriptionStore.Object, _consumerFactory.Object, _messageSerializer.Object);
        }

        [Fact]
        public async Task Can_Add_And_Start_New_Consumer_If_None_Found_For_Topic()
        {
            var subscription = A<Subscription>();
            var subs = new List<Subscription> { subscription };
            var mockConnectionManager = new Mock<IConnectionManager>();
            var mockSubscriberNotifierService = new Mock<ISubscriberNotifierService>();
            var topic = subscription.Topics.First();
            var consumer = new Mock<IConsumer>();

            _subscriptionStore.Setup(_ => _.GetSubscriptions()).Returns(new Dictionary<Topic, IConsumer>());
            _consumerFactory.Setup(_ => _.Create(It.IsAny<IConnectionManager>(), It.IsAny<ISubscriberNotifierService>(), It.IsAny<IPayloadSerializer>(), It.IsAny<Topic>())).Returns(consumer.Object);

            await _sut.AddSubscriptionsAsync(subs);
            _consumerFactory.Verify(_ => _.Create(It.IsAny<IConnectionManager>(), It.IsAny<ISubscriberNotifierService>(), It.IsAny<IPayloadSerializer>(), It.IsAny<Topic>()));
            consumer.Verify(_ => _.StartListeningAsync());
            _subscriptionStore.Verify(_ => _.AddCallbackToTopic(topic, subscription.Callback));
        }

        [Fact]
        public async Task Can_Update_Subscription_Callbacks_If_Consumer_Exists()
        {
            var topic = await SetupExistingConsumerWithCallback();
            var newSubscription = new Subscription()
            {
                Callback = A<Uri>(),
                Topics = new List<Topic> { topic }
            };

            var newSubs = new List<Subscription> { newSubscription };
            await _sut.AddSubscriptionsAsync(newSubs);

            _subscriptionStore.Verify(_ => _.AddCallbackToTopic(topic, newSubscription.Callback));
        }
        
        private async Task<Topic> SetupExistingConsumerWithCallback()
        {
            var subscription = A<Subscription>();
            var subs = new List<Subscription> { subscription };
            var mockConnectionManager = new Mock<IConnectionManager>();
            var mockSubscriberNotifierService = new Mock<ISubscriberNotifierService>();
            var topic = subscription.Topics.First();
            var consumer = new Mock<IConsumer>();


            _subscriptionStore.Setup(_ => _.GetSubscriptions()).Returns(new Dictionary<Topic, IConsumer>());
            _consumerFactory.Setup(_ => _.Create(It.IsAny<IConnectionManager>(), It.IsAny<ISubscriberNotifierService>(), It.IsAny<IPayloadSerializer>(), It.IsAny<Topic>())).Returns(consumer.Object);

            await _sut.AddSubscriptionsAsync(subs);

            var existingSubscriptions = new Dictionary<Topic, IConsumer>
            {
                { topic, consumer.Object }
            };
            _subscriptionStore.Setup(_ => _.GetSubscriptions()).Returns(existingSubscriptions);
            _consumerFactory.Setup(_ => _.Create(It.IsAny<IConnectionManager>(), It.IsAny<ISubscriberNotifierService>(), It.IsAny<IPayloadSerializer>(), It.IsAny<Topic>())).Returns(consumer.Object);

            return topic;
        }
    }
}
