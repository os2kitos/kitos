using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PubSub.Core.DomainModel.Repositories;
using PubSub.Core.DomainModel.Serializer;
using PubSub.Application.Services.RabbitMQUtils;
using PubSub.Core.DomainModel.Consumer;
using PubSub.Core.DomainModel.Notifier;
using Microsoft.Extensions.DependencyInjection;

namespace PubSub.Infrastructure.MessageQueue.Consumer
{
    public class RabbitMQConsumer : IConsumer
    {
        private readonly IRabbitMQConnectionManager _connectionManager;
        private readonly ISubscriberNotifier _subscriberNotifierService;
        private readonly string _topic;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IJsonPayloadSerializer _payloadSerializer;
        private IConnection _connection;
        private IChannel _channel;
        private IAsyncBasicConsumer _consumerCallback;

        public RabbitMQConsumer(IRabbitMQConnectionManager connectionManager, 
            ISubscriberNotifier subscriberNotifierService, 
            IJsonPayloadSerializer payloadSerializer, 
            string topic,
            IServiceScopeFactory serviceScopeFactory)
        {
            _connectionManager = connectionManager;
            _subscriberNotifierService = subscriberNotifierService;
            _topic = topic;
            this.serviceScopeFactory = serviceScopeFactory;
            _payloadSerializer = payloadSerializer;
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
                    using var scope = serviceScopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
                    var subscriptions = await repository.GetByTopic(_topic);
                    foreach (var callbackUrl in subscriptions.Select(x => x.Callback))
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
