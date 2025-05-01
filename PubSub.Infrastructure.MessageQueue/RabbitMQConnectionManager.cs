using PubSub.Application.Services.RabbitMQUtils;
using RabbitMQ.Client;

namespace PubSub.Infrastructure.MessageQueue
{
    public class RabbitMQConnectionManager : IRabbitMQConnectionManager
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection? _connection;

        public RabbitMQConnectionManager(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IConnection> GetConnectionAsync()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _connection = await _connectionFactory.CreateConnectionAsync();
            }
            return _connection;
        }

        public void Dispose()
        {
            _connection?.CloseAsync();
        }
    }
}
