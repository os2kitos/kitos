using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.Result;
using Microsoft.AspNet.OData;

namespace Presentation.Web
{
    [Authorize]
    public class BaseOdataController : ODataController
    {
        protected IHttpActionResult FromOperationFailure(OperationFailure failure)
        {
            return FromOperationError(failure);
        }

        protected IHttpActionResult FromOperationError(OperationError failure)
        {
            HttpStatusCode statusCode;
            switch (failure.FailureType)
            {
                case OperationFailure.BadInput:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                case OperationFailure.NotFound:
                    statusCode = HttpStatusCode.NotFound;
                    break;
                case OperationFailure.Forbidden:
                    statusCode = HttpStatusCode.Forbidden;
                    break;
                case OperationFailure.Conflict:
                    statusCode = HttpStatusCode.Conflict;
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            return ResponseMessage(new HttpResponseMessage(statusCode) {Content = new StringContent(failure.Message.GetValueOrFallback(statusCode.ToString("G")))});
        }
    }
}