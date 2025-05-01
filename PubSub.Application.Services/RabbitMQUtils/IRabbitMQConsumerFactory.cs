using PubSub.Core.DomainModel.Consumer;
using PubSub.Core.DomainModel.Notifier;
using PubSub.Core.DomainModel.Repositories;
using PubSub.Core.DomainModel.Serializer;

namespace PubSub.Application.Services.RabbitMQUtils
{
    public interface IRabbitMQConsumerFactory
    {
        IConsumer Create(IRabbitMQConnectionManager connectionManager, ISubscriberNotifier subscriberNotifierService, IJsonPayloadSerializer payloadSerializer, string topic, ISubscriptionRepositoryProvider subscriptionRepositoryProvider);
    }
}
