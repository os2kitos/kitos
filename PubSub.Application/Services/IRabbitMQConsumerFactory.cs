using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;

namespace PubSub.Application.Services
{
    public interface IRabbitMQConsumerFactory
    {
        IConsumer Create(IConnectionManager connectionManager, ISubscriberNotifierService subscriberNotifierService, IPayloadSerializer payloadSerializer, string topic, IServiceScopeFactory scopeFactory);
    }
}
