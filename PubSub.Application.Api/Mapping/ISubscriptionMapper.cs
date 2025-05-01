using PubSub.Application.Api.DTOs.Request;
using PubSub.Application.Api.DTOs.Response;
using PubSub.Core.DomainModel.Parameters;
using PubSub.Core.DomainModel.Subscriptions;

namespace PubSub.Application.Api.Mapping;

public interface ISubscriptionMapper
{
    SubscriptionResponseDTO ToResponseDTO(Subscription subscription);

    IEnumerable<CreateSubscriptionParameters> FromDTO(SubscribeRequestDto dto);
}