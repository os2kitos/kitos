using Moq;
using PubSub.Application.Services;
using RabbitMQ.Client;

namespace PubSub.Test.Unit.Core
{
    public class RabbitMQConnectionManagerTest
    {
        private Mock<IConnectionFactory> _connectionFactory;
        private RabbitMQConnectionManager _sut;

        public RabbitMQConnectionManagerTest()
        {
            _connectionFactory = new Mock<IConnectionFactory>();
            _sut = new RabbitMQConnectionManager(_connectionFactory.Object);
        }

        [Fact]
        public async Task Creates_Connection_If_Null()
        {
            var expected = SetupFactoryReturnsOpenConnection();
            var actual = await _sut.GetConnectionAsync();

            Assert.Equal(expected.Object, actual);
            _connectionFactory.Verify(_ => _.CreateConnectionAsync(default), Times.Once);
        }

        [Fact]
        public async Task Creates_Connection_If_Existing_IsClosed()
        {
            var toBeClosedMock = new Mock<IConnection>();
            _connectionFactory.Setup(_ => _.CreateConnectionAsync(default)).ReturnsAsync(toBeClosedMock.Object);
            await _sut.GetConnectionAsync();
            await toBeClosedMock.Object.CloseAsync();
            var expected = SetupFactoryReturnsOpenConnection();

            var actual = await _sut.GetConnectionAsync();

            Assert.NotNull(actual);
            Assert.Equal(expected.Object, actual);
        }

        [Fact]
        public async Task Returns_Cached_Connection_If_Any()
        {
            var expected = new Mock<IConnection>();
            _connectionFactory.Setup(_ => _.CreateConnectionAsync(default)).ReturnsAsync(expected.Object);
            
            var actual = await _sut.GetConnectionAsync();

            Assert.Equal(expected.Object, actual);
        }

        private Mock<IConnection> SetupFactoryReturnsOpenConnection()
        {
            var newConnection = new Mock<IConnection>();
            _connectionFactory.Setup(_ => _.CreateConnectionAsync(default)).ReturnsAsync(newConnection.Object);
            return newConnection;
        }
    }
}
