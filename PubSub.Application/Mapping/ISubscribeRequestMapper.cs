using PubSub.Application.DTOs;
using PubSub.Core.Models;

namespace PubSub.Application.Mapping
{
    public interface ISubscribeRequestMapper
    {
        Subscription FromDto(SubscribeRequestDto dto);
    }
}
