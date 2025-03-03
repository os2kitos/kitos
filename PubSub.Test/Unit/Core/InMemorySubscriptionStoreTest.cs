using Moq;
using PubSub.Core.Consumers;
using PubSub.Core.Models;
using PubSub.Core.Services.Subscribe;
using PubSub.Test.Base.Tests.Toolkit.Patterns;

namespace PubSub.Test.Unit.Core
{
    public class InMemorySubscriptionStoreTest: WithAutoFixture
    {
        private InMemorySubscriptionStore _sut;

        public InMemorySubscriptionStoreTest()
        {
            _sut = new InMemorySubscriptionStore();
        }

        [Fact]
        public void Can_Add_New_Topic()
        {
            var topic = A<Topic>();
            var consumer = new Mock<IConsumer>();

            _sut.SetConsumerForTopic(topic, consumer.Object);

            var subscriptions = _sut.GetSubscriptions();
            Assert.Equal(subscriptions[topic],  consumer.Object);
        }

        [Fact]
        public void Updates_Consumer_If_Topic_Exists()
        {
            var topic = A<Topic>();
            var consumer = new Mock<IConsumer>();
            _sut.SetConsumerForTopic(topic, consumer.Object);
            var secondConsumer = new Mock<IConsumer>();

            _sut.SetConsumerForTopic(topic, secondConsumer.Object);

            var subscriptions = _sut.GetSubscriptions();
            Assert.Equal(subscriptions[topic], secondConsumer.Object);
        }

        [Fact]
        public void Can_Add_Callback_To_Consumer_By_Topic()
        {
            var (topic, callback, consumer) = SetupCreateConsumer();

            _sut.AddCallbackToTopic(topic,callback);

            consumer.Verify(_ => _.AddCallbackUrl(callback));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Check_If_Has_Topic(bool expected) {
            var topic = A<Topic>();
            var consumer = new Mock<IConsumer>();
            if (expected)
            {
                _sut.SetConsumerForTopic(topic, consumer.Object);
            }

            var actual = _sut.HasTopic(topic);

            Assert.Equal(expected, actual);
        }

        private (Topic topic, Uri callback, Mock<IConsumer> consumer) SetupCreateConsumer()
        {
            var topic = A<Topic>();
            var consumer = new Mock<IConsumer>();
            var callback = A<Uri>();
            _sut.SetConsumerForTopic(topic, consumer.Object);
            return (topic, callback, consumer);
        }
    }
}
