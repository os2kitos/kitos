using PubSub.Application.Services.RabbitMQUtils;
using PubSub.Core.DomainModel.Consumer;
using PubSub.Core.DomainModel.Notifier;
using PubSub.Core.DomainModel.Serializer;
using Microsoft.Extensions.DependencyInjection;


namespace PubSub.Infrastructure.MessageQueue.Consumer
{
    public class RabbitMQConsumerFactory : IRabbitMQConsumerFactory
    {
        public IConsumer Create(IRabbitMQConnectionManager connectionManager, ISubscriberNotifier subscriberNotifierService, IJsonPayloadSerializer payloadSerializer, string topic, IServiceScopeFactory serviceScopeFactory)
        {
            return new RabbitMQConsumer(connectionManager, subscriberNotifierService, payloadSerializer, topic, serviceScopeFactory);
        }
    }
}
