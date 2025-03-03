using PubSub.Application.DTOs;
using PubSub.Core.Models;

namespace PubSub.Application.Mapping
{
    public class PublishRequestMapper : IPublishRequestMapper
    {
        public Publication FromDto(PublishRequestDto dto)
        {
            return new Publication(new Topic(dto.Topic), dto.Message);
        }
    }
}
