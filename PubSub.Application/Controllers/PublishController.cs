using Microsoft.AspNetCore.Mvc;
using PubSub.Application.DTOs;
using PubSub.Application.Mapping;
using PubSub.Core.Services.Publisher;

namespace PubSub.Application.Controllers
{
    [ApiController]
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
        public async Task<IActionResult> Publish(PublishRequestDto request) {
            if (!ModelState.IsValid) return BadRequest("Invalid request object provided.");

            var publication = _publishRequestMapper.FromDto(request);
            await _publisherService.Publish(publication);

            return NoContent();
        }
    }
}
