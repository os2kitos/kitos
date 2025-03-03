using PubSub.Core.Managers;
using PubSub.Core.Models;
using PubSub.Core.Services.Notifier;
using PubSub.Core.Services.Serializer;

namespace PubSub.Core.Consumers
{
    public class RabbitMQConsumerFactory : IRabbitMQConsumerFactory
    {
        public IConsumer Create(IConnectionManager connectionManager, ISubscriberNotifierService subscriberNotifierService, IMessageSerializer messageSerializer, Topic topic)
        {
            return new RabbitMQConsumer(connectionManager, subscriberNotifierService, messageSerializer, topic);
        }
    }
}
