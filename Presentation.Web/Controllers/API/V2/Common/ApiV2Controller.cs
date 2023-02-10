using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.Abstractions.Types;
using Presentation.Web.Extensions;

namespace Presentation.Web.Controllers.API.V2.Common
{
    [Authorize]
    public abstract class ApiV2Controller: ApiController
    {
        protected IHttpActionResult FromOperationFailure(OperationFailure failure)
        {
            return FromOperationError(failure);
        }

        protected IHttpActionResult FromOperationError(OperationError failure)
        {
            var statusCode = failure.FailureType.ToHttpStatusCode();

            return ResponseMessage(new HttpResponseMessage(statusCode) { Content = new StringContent(failure.Message.GetValueOrFallback(statusCode.ToString("G"))) });
        }

        protected IHttpActionResult NoContent()
        {
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}