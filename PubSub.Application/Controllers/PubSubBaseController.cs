using Microsoft.AspNetCore.Mvc;
using PubSub.Core.Abstractions.ErrorTypes;

namespace PubSub.Application.Controllers;

public class PubSubBaseController : ControllerBase
{
    protected IActionResult FromOperationError(OperationError error)
    {
        return error switch
        {
            OperationError.Duplicate => Conflict(),
            OperationError.NotFound => NotFound(),
            OperationError.Forbidden => Forbid(),
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
        };
    }
}