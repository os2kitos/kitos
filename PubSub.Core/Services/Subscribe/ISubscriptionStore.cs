using PubSub.Core.Consumers;
using PubSub.Core.Models;

namespace PubSub.Core.Services.Subscribe
{
    public interface ISubscriptionStore
    {
        void AddCallbackToTopic(Topic topic, Uri callback);

        void SetConsumerForTopic(Topic topic, IConsumer consumer);

        IDictionary<Topic, IConsumer> GetSubscriptions();

        bool HasTopic(Topic topic);
    }
}
