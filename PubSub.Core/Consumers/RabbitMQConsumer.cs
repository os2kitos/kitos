using System.Text;
using PubSub.Core.Managers;
using PubSub.Core.Models;
using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PubSub.Core.Consumers
{
    public class RabbitMQConsumer : IConsumer
    {
        private readonly IConnectionManager _connectionManager;
        private readonly ISubscriberNotifierService _subscriberNotifierService;
        private readonly Topic _topic;
        private readonly IPayloadSerializer _payloadSerializer;
        private readonly HashSet<Uri> _callbackUrls = new();
        private IConnection _connection;
        private IChannel _channel;
        private IAsyncBasicConsumer _consumerCallback;


        public RabbitMQConsumer(IConnectionManager connectionManager, ISubscriberNotifierService subscriberNotifierService, IPayloadSerializer payloadSerializer, Topic topic)
        {
            _connectionManager = connectionManager;
            _subscriberNotifierService = subscriberNotifierService;
            _topic = topic;
            _payloadSerializer = payloadSerializer;
        }

        public async Task StartListeningAsync()
        {
            var topicName = _topic.Name;
            _connection = await _connectionManager.GetConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(topicName, true, false, false);
            _consumerCallback = GetConsumerCallback();

            await _channel.BasicConsumeAsync(topicName, autoAck: true, consumer: _consumerCallback);
        }

        private IAsyncBasicConsumer GetConsumerCallback() {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var payload = _payloadSerializer.Deserialize(body);

                    foreach (var callbackUrl in _callbackUrls)
                    {
                        await _subscriberNotifierService.Notify(payload, callbackUrl.AbsoluteUri);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in consumer: {ex.Message}\n{ex.StackTrace}");
                }
            
        };
            return consumer;
        }

        public void AddCallbackUrl(Uri callbackUrl)
        {
            _callbackUrls.Add(callbackUrl);
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }
}
