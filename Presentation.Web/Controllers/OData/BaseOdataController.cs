using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.DomainModel.Result;
using Microsoft.AspNet.OData;

namespace Presentation.Web.Controllers.OData
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
            HttpStatusCode statusCode = failure.FailureType switch
            {
                OperationFailure.BadInput => HttpStatusCode.BadRequest,
                OperationFailure.NotFound => HttpStatusCode.NotFound,
                OperationFailure.Forbidden => HttpStatusCode.Forbidden,
                OperationFailure.Conflict => HttpStatusCode.Conflict,
                _ => HttpStatusCode.InternalServerError
            };

            return ResponseMessage(new HttpResponseMessage(statusCode) {Content = new StringContent(failure.Message.GetValueOrFallback(statusCode.ToString("G")))});
        }
    }
}