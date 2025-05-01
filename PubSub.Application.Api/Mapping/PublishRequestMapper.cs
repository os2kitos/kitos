using PubSub.Application.Api.DTOs.Request;
using PubSub.Core.DomainModel.Topics;
using PubSub.Core.DomainModel.Publications;

namespace PubSub.Application.Api.Mapping
{
    public class PublishRequestMapper : IPublishRequestMapper
    {
        public Publication FromDto(PublishRequestDto dto)
        {
            return new Publication(new Topic(dto.Topic), dto.Payload);
        }
    }
}
