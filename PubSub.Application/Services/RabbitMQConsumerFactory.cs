using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;

namespace PubSub.Application.Services
{
    public class RabbitMQConsumerFactory : IRabbitMQConsumerFactory
    {
        public IConsumer Create(IConnectionManager connectionManager, ISubscriberNotifierService subscriberNotifierService, IPayloadSerializer payloadSerializer, string topic, IServiceScopeFactory scopeFactory)
        {
            return new RabbitMQConsumer(connectionManager, subscriberNotifierService, payloadSerializer, topic, scopeFactory);
        }
    }
}
