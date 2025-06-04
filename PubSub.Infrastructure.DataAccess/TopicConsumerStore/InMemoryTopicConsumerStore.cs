using PubSub.Core.DomainModel.Consumer;
using PubSub.Core.DomainServices;
using System.Collections.Concurrent;

namespace PubSub.Infrastructure.DataAccess.TopicConsumerStore
{
    public class InMemoryTopicConsumerStore : ITopicConsumerStore
    {
        private readonly ConcurrentDictionary<string, IConsumer> _consumersByTopicDictionary = new();

        public void SetConsumerForTopic(string topic, IConsumer consumer)
        {
            _consumersByTopicDictionary[topic] = consumer;
        }

        public bool HasConsumer(string topic)
        {
            return _consumersByTopicDictionary.ContainsKey(topic);
        }
    }
}
