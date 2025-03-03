using Moq;
using PubSub.Test.Base.Tests.Toolkit.Patterns;
using RabbitMQ.Client;

namespace PubSub.Test.Base
{
    public class TestClassWithIChannelBase: WithAutoFixture
    {
        protected void AssertChannelDeclaresQueue(Mock<IChannel> channel)
        {
            channel.Verify(ch => ch.QueueDeclareAsync(
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<IDictionary<string, object?>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        protected void AssertChannelConsumes(Mock<IChannel> channel)
        {
            channel.Verify(ch => ch.BasicConsumeAsync(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object?>>(),
                It.IsAny<IAsyncBasicConsumer>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }
    }
}
