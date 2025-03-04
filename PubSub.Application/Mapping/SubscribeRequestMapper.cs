using PubSub.Application.DTOs;
using PubSub.Core.Models;

namespace PubSub.Application.Mapping
{
    public class SubscribeRequestMapper : ISubscribeRequestMapper
    {
        public Subscription FromDto(SubscribeRequestDto dto)
        {
            return new Subscription()
            {
                Callback = dto.Callback,
                Topics = dto.Topics.Select(t => new Topic(t))
            };
        }
    }
}
