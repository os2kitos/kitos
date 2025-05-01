using PubSub.Core.DomainModel.Consumer;

namespace PubSub.Core.DomainServices
{
    public interface ITopicConsumerStore
    {
        void SetConsumerForTopic(string topic, IConsumer consumer);

        bool HasConsumer(string topic);
    }
}
