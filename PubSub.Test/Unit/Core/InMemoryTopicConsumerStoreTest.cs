using Moq;
using PubSub.Application.Services;
using PubSub.Core.Models;
using PubSub.Test.Base.Tests.Toolkit.Patterns;

namespace PubSub.Test.Unit.Core
{
    public class InMemoryTopicConsumerStoreTest: WithAutoFixture
    {
        private InMemoryTopicConsumerStore _sut;

        public InMemoryTopicConsumerStoreTest()
        {
            _sut = new InMemoryTopicConsumerStore();
        }

        [Fact]
        public void Can_Add_New_Topic()
        {
            var topic = A<string>();
            var consumer = new Mock<IConsumer>();

            _sut.SetConsumerForTopic(topic, consumer.Object);

            Assert.True(_sut.HasConsumer(topic));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_Check_If_Has_Topic(bool expected) {
            var topic = A<string>();
            var consumer = new Mock<IConsumer>();
            if (expected)
            {
                _sut.SetConsumerForTopic(topic, consumer.Object);
            }

            var actual = _sut.HasConsumer(topic);

            Assert.Equal(expected, actual);
        }

    }
}
