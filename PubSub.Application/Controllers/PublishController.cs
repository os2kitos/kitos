using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PubSub.Application.DTOs;
using PubSub.Application.Mapping;
using PubSub.Core.Services.Publisher;

namespace PubSub.Application.Controllers
{
    [ApiController]
    [Authorize(Policy = Constants.Config.Validation.CanPublishPolicy)]
    [Route("api/publish")]
    public class PublishController: ControllerBase
    {
        private readonly IPublisherService _publisherService;
        private readonly IPublishRequestMapper _publishRequestMapper;

        public PublishController(IPublisherService publisherService, IPublishRequestMapper publishRequestMapper)
        {
            _publisherService = publisherService;
            _publishRequestMapper = publishRequestMapper;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Publish(PublishRequestDto request) {
            if (!ModelState.IsValid) return BadRequest("Invalid request object provided.");

            var publication = _publishRequestMapper.FromDto(request);
            await _publisherService.PublishAsync(publication);

            return NoContent();
        }
    }
}
