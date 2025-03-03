using Microsoft.AspNetCore.Mvc;
using PubSub.Application.DTOs;
using PubSub.Application.Mapping;
using PubSub.Core.Models;
using PubSub.Core.Services.Subscribe;


namespace PubSub.Application.Controllers;

[ApiController]

[Route("api/subscribe")]
public class SubscribeController : ControllerBase
{
    private readonly ISubscriberService _subscriberService;
    private readonly ISubscribeRequestMapper _subscribeRequestMapper;

    public SubscribeController(ISubscriberService subscriberService, ISubscribeRequestMapper subscribeRequestMapper)
    {
        _subscriberService = subscriberService;
        _subscribeRequestMapper = subscribeRequestMapper;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest();
        var subscription = _subscribeRequestMapper.FromDto(request);
        var subscriptions = new List<Subscription>() { subscription };
        await _subscriberService.AddSubscriptionsAsync(subscriptions);
        return NoContent();
    }
}
