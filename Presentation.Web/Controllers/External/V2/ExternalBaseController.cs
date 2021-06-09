using Core.DomainModel.Result;
using Presentation.Web.Extensions;
using Presentation.Web.Models;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Presentation.Web.Controllers.External.V2
{
    public class ExternalBaseController: ApiController
    {
        protected HttpResponseMessage CreateResponse<T>(HttpStatusCode statusCode, T response, string msg = "")
        {
            var wrap = new ApiReturnDTO<T>
            {
                Msg = msg,
                Response = response
            };

            return Request.CreateResponse(statusCode, wrap);
        }

        protected HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string msg = "")
        {
            return CreateResponse(statusCode, new object(), msg);
        }

        protected HttpResponseMessage FromOperationError(OperationError failure)
        {
            var statusCode = failure.FailureType.ToHttpStatusCode();

            return CreateResponse(statusCode, failure.Message.GetValueOrFallback(string.Empty));
        }
        protected new HttpResponseMessage Ok<T>(T response)
        {
            return CreateResponse(HttpStatusCode.OK, response);
        }
    }
}