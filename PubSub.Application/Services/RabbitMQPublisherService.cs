using PubSub.Core.Models;
using PubSub.Core.Services.Serializer;
using RabbitMQ.Client;

namespace PubSub.Application.Services
{
    public class RabbitMQPublisherService : IPublisherService
    {
        private readonly IConnectionManager _connectionManager;
        private readonly IPayloadSerializer _payloadSerializer;

        public RabbitMQPublisherService(IConnectionManager connectionManager, IPayloadSerializer payloadSerializer)
        {
            _connectionManager = connectionManager;
            _payloadSerializer = payloadSerializer;
        }

        public async Task PublishAsync(Publication publication)
        {
            var topic = publication.Topic;
            var connection = await _connectionManager.GetConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: topic.Name, durable: true, exclusive: false, autoDelete: false);

            var serializedBody = _payloadSerializer.Serialize(publication.Payload);
            var properties = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: topic.Name, true, basicProperties: properties, body: serializedBody);
        }
    }
}
