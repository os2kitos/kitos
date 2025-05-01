using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PubSub.Application.Api.DTOs.Request;
using PubSub.Application.Api.Mapping;
using PubSub.Core.DomainModel.Publisher;
using PubSub.Core.DomainServices;


namespace PubSub.Application.Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)] 
    [ApiController]
    [Authorize(Policy = Constants.Config.Validation.CanPublishPolicy)]
    [Route("api/publish")]
    public class PublishController: ControllerBase
    {
        private readonly IPublisher _publisherService;
        private readonly IPublishRequestMapper _publishRequestMapper;
        private readonly ITopicConsumerInstantiatorService _topicConsumerInstantiatorService;

        public PublishController(IPublisher publisherService, IPublishRequestMapper publishRequestMapper, ITopicConsumerInstantiatorService topicConsumerInstantiatorService)
        {
            _publisherService = publisherService;
            _publishRequestMapper = publishRequestMapper;
            _topicConsumerInstantiatorService = topicConsumerInstantiatorService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Publish(PublishRequestDto request) {
            if (!ModelState.IsValid) return BadRequest("Invalid request object provided.");

            var publication = _publishRequestMapper.FromDto(request);
            await _publisherService.PublishAsync(publication);
            await _topicConsumerInstantiatorService.InstantiateTopic(request.Topic);

            return NoContent();
        }
    }
}
