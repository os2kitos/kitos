using System.Net;
using Core.Abstractions.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V2.Common
{
    [Authorize]
    [V2StyleJsonResponseSerialization]
    public abstract class ApiV2Controller : ControllerBase
    {
        protected IActionResult FromOperationFailure(OperationFailure failure)
        {
            return FromOperationError(failure);
        }

        protected IActionResult FromOperationError(OperationError failure)
        {
            var statusCode = failure.FailureType.ToHttpStatusCode();
            var message = failure.Message.GetValueOrFallback(((HttpStatusCode)statusCode).ToString("G"));
            return StatusCode((int)statusCode, message);
        }

        protected new IActionResult NoContent()
        {
            return base.NoContent();
        }

        /// <summary>
        /// Convenience wrapper for <see cref="NoContent()"/>
        /// </summary>
        protected IActionResult NoContent(object ignored)
        {
            return NoContent();
        }
    }
}
