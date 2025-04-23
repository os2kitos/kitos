using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;
using PubSub.DataAccess;
using PubSub.DataAccess.Repositories;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PubSub.Application.Services
{
    public class RabbitMQConsumer : IConsumer
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ISubscriberNotifierService _subscriberNotifierService;
        private readonly string _topic;
        private readonly IPayloadSerializer _payloadSerializer;
        private IConnection _connection;
        private IChannel _channel;
        private IAsyncBasicConsumer _consumerCallback;
        private readonly IServiceScopeFactory _scopeFactory;


        public RabbitMQConsumer(IConnectionManager connectionManager, ISubscriberNotifierService subscriberNotifierService, IPayloadSerializer payloadSerializer, string topic, IServiceScopeFactory scopeFactory)
        {
            _connectionManager = connectionManager;
            _subscriberNotifierService = subscriberNotifierService;
            _topic = topic;
            _payloadSerializer = payloadSerializer;
            _scopeFactory = scopeFactory;
        }

        public async Task StartListeningAsync()
        {
            var topicName = _topic;
            _connection = await _connectionManager.GetConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(topicName, true, false, false);
            _consumerCallback = GetConsumerCallback();

            await _channel.BasicConsumeAsync(topicName, autoAck: true, consumer: _consumerCallback);
        }

        private AsyncEventingBasicConsumer GetConsumerCallback()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var payload = _payloadSerializer.Deserialize(body);
                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
                    var subs = await repo.GetByTopic(_topic);
                    foreach (var callbackUrl in subs.Select(x => x.Callback))
                    {
                        await _subscriberNotifierService.Notify(payload, callbackUrl);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in consumer: {ex.Message}\n{ex.StackTrace}");
                }
            };
            return consumer;
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
