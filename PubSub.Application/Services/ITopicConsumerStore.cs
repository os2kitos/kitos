namespace PubSub.Application.Services
{
    public interface ITopicConsumerStore
    {
        void SetConsumerForTopic(string topic, IConsumer consumer);

        bool HasConsumer(string topic);
    }
}
