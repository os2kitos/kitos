using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.Result;
using Presentation.Web.Extensions;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V2.External
{
    [PublicApi(true)]
    [Authorize]
    public class ExternalBaseController: ApiController
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
    }
}