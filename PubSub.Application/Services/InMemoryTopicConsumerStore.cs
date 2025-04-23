using System.Collections.Concurrent;

namespace PubSub.Application.Services
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
