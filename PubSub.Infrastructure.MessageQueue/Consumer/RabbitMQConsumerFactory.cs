using PubSub.Application.Services.RabbitMQUtils;
using PubSub.Core.DomainModel.Consumer;
using PubSub.Core.DomainModel.Notifier;
using PubSub.Core.DomainModel.Repositories;
using PubSub.Core.DomainModel.Serializer;

namespace PubSub.Infrastructure.MessageQueue.Consumer
{
    public class RabbitMQConsumerFactory : IRabbitMQConsumerFactory
    {
        public IConsumer Create(IRabbitMQConnectionManager connectionManager, ISubscriberNotifier subscriberNotifierService, IJsonPayloadSerializer payloadSerializer, string topic, ISubscriptionRepositoryProvider subscriptionRepositoryProvider)
        {
            return new RabbitMQConsumer(connectionManager, subscriberNotifierService, payloadSerializer, topic, subscriptionRepositoryProvider);
        }
    }
}
