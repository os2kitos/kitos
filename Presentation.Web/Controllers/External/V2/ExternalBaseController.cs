using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.Result;
using Presentation.Web.Extensions;

namespace Presentation.Web.Controllers.External.V2
{
    [Authorize]
    public class ExternalBaseController: ApiController
    {
        protected IHttpActionResult FromOperationFailure(OperationFailure failure)
        {
            return FromOperationError(failure);
        }

        protected IHttpActionResult FromOperationError(OperationError failure)
        {
            HttpStatusCode statusCode = failure.FailureType.ToHttpStatusCode();

            return ResponseMessage(new HttpResponseMessage(statusCode) { Content = new StringContent(failure.Message.GetValueOrFallback(statusCode.ToString("G"))) });
        }
    }
}