using CSharpFunctionalExtensions;
using PubSub.Core.Abstractions.ErrorTypes;
using PubSub.Core.DomainModel.Parameters;
using PubSub.Core.DomainModel.Subscriptions;

namespace PubSub.Core.DomainServices;

public interface ISubscriptionService
{
    public Task AddSubscriptionsAsync(IEnumerable<CreateSubscriptionParameters> request);
    public Task<IEnumerable<Subscription>> GetActiveUserSubscriptions();
    public Task<Maybe<OperationError>> DeleteSubscription(Guid uuid);
}