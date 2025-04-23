using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PubSub.Application.DTOs;
using PubSub.Application.Mapping;
using PubSub.Application.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace PubSub.Application.Controllers;

[ApiController]
[Authorize]
[Route("api/subscribe")]
public class SubscribeController : PubSubBaseController
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ISubscriptionMapper _subscriptionMapper;

    public SubscribeController(ISubscriptionService subscriptionService, ISubscriptionMapper subscriptionMapper)
    {
        _subscriptionService = subscriptionService;
        _subscriptionMapper = subscriptionMapper;
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Establishes a new subscription.",
        Description = "Calling this endpoint multiple times with the same (Topic, CallbackUrl) pair will not create duplicates. " +
                      "The subscription is owned by the user represented by the token."
        )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest();
        var subscriptionCreationRequests = _subscriptionMapper.FromDTO(request);
        await _subscriptionService.AddSubscriptionsAsync(subscriptionCreationRequests);
        return NoContent();
    }

    [SwaggerOperation(
        Summary = "Deletes an existing subscription by its UUID.",
        Description = "You can only delete subscriptions that belong to you. "
    )]
    [HttpDelete]
    [Route("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid uuid)
    {
        var result = await _subscriptionService.DeleteSubscription(uuid);
        return result.Match(FromOperationError, NoContent);
    }

    [SwaggerOperation(
        Summary = "Retrieves all active subscriptions owned by the current user represented by the token."
    )]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveSubscriptions()
    {
        var subscriptions = await _subscriptionService.GetActiveUserSubscriptions();
        return Ok(subscriptions.Select(_subscriptionMapper.ToResponseDTO));
    }

}
