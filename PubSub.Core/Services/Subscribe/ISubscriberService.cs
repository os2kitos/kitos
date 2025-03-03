using PubSub.Core.Models;

namespace PubSub.Core.Services.Subscribe
{
    public interface ISubscriberService
    {
        Task AddSubscriptionsAsync(IEnumerable<Subscription> subscriptions);
    }
}
