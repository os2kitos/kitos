using System;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Presentation.Web.Helpers;
using Presentation.Web.Models;

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

        protected HttpResponseMessage CreateResponse(HttpStatusCode statusCode, Exception e)
        {
            return CreateResponse(statusCode, e, e.Message);
        }


        protected HttpResponseMessage Created<T>(T response, Uri location = null)
        {
            var result = CreateResponse(HttpStatusCode.Created, response);
            if (location != null)
                result.Headers.Location = location;

            return result;
        }

        protected new HttpResponseMessage Ok()
        {
            return CreateResponse(HttpStatusCode.OK);
        }

        protected new HttpResponseMessage Ok<T>(T response)
        {
            return CreateResponse(HttpStatusCode.OK, response);
        }

        protected virtual HttpResponseMessage Error<T>(T response)
        {
            if (response is SecurityException)
            {
                return Unauthorized();
            }

            return CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        protected new virtual HttpResponseMessage BadRequest(string message = "")
        {
            return CreateResponse(HttpStatusCode.BadRequest, message);
        }

        protected virtual HttpResponseMessage Unauthorized()
        {
            return CreateResponse(HttpStatusCode.Unauthorized, Constants.StatusCodeMessages.UnauthorizedErrorMessage);
        }

        protected virtual HttpResponseMessage Unauthorized<T>(T response)
        {
            return CreateResponse(HttpStatusCode.Unauthorized, response);
        }

        protected HttpResponseMessage NoContent()
        {
            return CreateResponse(HttpStatusCode.NoContent);
        }

        protected new HttpResponseMessage NotFound()
        {
            return CreateResponse(HttpStatusCode.NotFound);
        }

        protected HttpResponseMessage Conflict(string msg)
        {
            return CreateResponse(HttpStatusCode.Conflict, msg);
        }

        protected HttpResponseMessage NotAllowed()
        {
            return CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        protected HttpResponseMessage Forbidden()
        {
            return CreateResponse(HttpStatusCode.Forbidden, Constants.StatusCodeMessages.ForbiddenErrorMessage);
        }

        protected HttpResponseMessage Forbidden(string msg)
        {
            return CreateResponse(HttpStatusCode.Forbidden, msg);
        }
    }
}