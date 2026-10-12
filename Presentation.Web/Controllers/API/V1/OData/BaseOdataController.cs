using Core.Abstractions.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Presentation.Web.Extensions;

namespace Presentation.Web.Controllers.API.V1.OData
{
    [Authorize]
    public class BaseOdataController : ODataController
    {
        protected IActionResult FromOperationFailure(OperationFailure failure)
        {
            return FromOperationError(failure);
        }

        protected IActionResult FromOperationError(OperationError failure)
        {
            var statusCode = failure.FailureType.ToHttpStatusCode();
            return StatusCode((int)statusCode, failure.Message.GetValueOrFallback(statusCode.ToString("G")));
        }
    }
}
