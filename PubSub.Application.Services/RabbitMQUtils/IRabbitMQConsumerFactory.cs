using Microsoft.Extensions.DependencyInjection;
using PubSub.Core.DomainModel.Consumer;
using PubSub.Core.DomainModel.Notifier;
using PubSub.Core.DomainModel.Serializer;

namespace PubSub.Application.Services.RabbitMQUtils
{
    public interface IRabbitMQConsumerFactory
    {
        IConsumer Create(IRabbitMQConnectionManager connectionManager, ISubscriberNotifier subscriberNotifierService, IJsonPayloadSerializer payloadSerializer, string topic, IServiceScopeFactory serviceScopeFactory);
    }
}
