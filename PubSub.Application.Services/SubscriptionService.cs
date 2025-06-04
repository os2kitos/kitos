using CSharpFunctionalExtensions;
using PubSub.Application.Services.CurrentUserService;
using PubSub.Core.Abstractions.ErrorTypes;
using PubSub.Core.DomainModel.Parameters;
using PubSub.Core.DomainModel.Repositories;
using PubSub.Core.DomainModel.Subscriptions;
using PubSub.Core.DomainServices;

namespace PubSub.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITopicConsumerInstantiatorService _topicConsumerInstantiatorService;

    public SubscriptionService(ISubscriptionRepository repository, ICurrentUserService currentUserService, ITopicConsumerInstantiatorService topicConsumerInstantiatorService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _topicConsumerInstantiatorService = topicConsumerInstantiatorService;
    }
    public async Task AddSubscriptionsAsync(IEnumerable<CreateSubscriptionParameters> requests)
    {
        var maybeUserId = _currentUserService.UserId;
        if (maybeUserId.HasValue)
        {
            foreach (var request in requests)
            {
                var exists = await _repository.Exists(request.Topic, request.Callback);
                if (exists) continue;
                var newSubscription = ToSubscriptionWithOwnerId(request, maybeUserId.Value);
                await _repository.AddAsync(newSubscription);
                await _topicConsumerInstantiatorService.InstantiateTopic(newSubscription.Topic);
            }
        }
    }

    public Task<IEnumerable<Subscription>> GetActiveUserSubscriptions()
    {
        return _repository.GetAllByUserId(_currentUserService.UserId.Value);
    }

    public async Task<Maybe<OperationError>> DeleteSubscription(Guid uuid)
    {
        var subscriptionMaybe = await _repository.GetAsync(uuid);
        if (subscriptionMaybe.HasNoValue)
        {
            return OperationError.NotFound;
        }
        if (!CanDeleteSubscription(subscriptionMaybe.Value))
        {
            return OperationError.Forbidden;
        }

        await _repository.DeleteAsync(subscriptionMaybe.Value);
        return Maybe<OperationError>.None;
    }

    private bool CanDeleteSubscription(Subscription subscription)
    {
        return subscription.OwnerId == _currentUserService.UserId;
    }

    private static Subscription ToSubscriptionWithOwnerId(CreateSubscriptionParameters parameters, string userId)
    {
        return new Subscription(parameters.Callback, parameters.Topic, userId);
    }
}