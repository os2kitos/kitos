using PubSub.Application.Api.DTOs.Request;
using PubSub.Application.Api.DTOs.Response;
using PubSub.Core.DomainModel.Parameters;
using PubSub.Core.DomainModel.Subscriptions;

namespace PubSub.Application.Api.Mapping;

public class SubscriptionMapper : ISubscriptionMapper
{
    public SubscriptionResponseDTO ToResponseDTO(Subscription subscription)
    {
        return new SubscriptionResponseDTO
        {
            Uuid = subscription.Uuid,
            CallbackUrl = subscription.Callback,
            Topic = subscription.Topic
        };
    }

    public IEnumerable<CreateSubscriptionParameters> FromDTO(SubscribeRequestDto dto)
    {
        return dto.Topics.Select(topic => new CreateSubscriptionParameters(dto.Callback.AbsoluteUri, topic)).ToList();
    }
}