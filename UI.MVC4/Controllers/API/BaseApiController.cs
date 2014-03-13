using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public abstract class BaseApiController : ApiController
    {
        protected HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string msg = "")
        {
            var wrap = new ApiReturnDTO<Object>
                {
                    Msg = msg,
                    Response = {}
                };

            return Request.CreateResponse(statusCode, wrap);
        }

        protected HttpResponseMessage CreateResponse(HttpStatusCode statusCode, Exception e)
        {
            var wrap = new ApiReturnDTO<Exception>
                {
                    Msg = e.Message,
                    Response = e
                };

            return Request.CreateResponse(statusCode, wrap);
        }

        protected HttpResponseMessage CreateResponse<T>(HttpStatusCode statusCode, T response, string msg = "")
        {
            var wrap = new ApiReturnDTO<T>
                {
                    Msg = msg,
                    Response = response
                };

            return Request.CreateResponse(statusCode, wrap);
        }

        protected HttpResponseMessage Created<T>(T response)
        {
            return CreateResponse(HttpStatusCode.Created, response);
        }

        protected HttpResponseMessage Ok()
        {
            return CreateResponse(HttpStatusCode.OK);
        }

        protected HttpResponseMessage Ok<T>(T response)
        {
            return CreateResponse(HttpStatusCode.OK, response);
        }

        protected HttpResponseMessage Error<T>(T response)
        {
            return CreateResponse(HttpStatusCode.InternalServerError, response);
        }

        protected HttpResponseMessage Unauthorized<T>()
        {
            return CreateResponse(HttpStatusCode.Unauthorized);
        }

        protected HttpResponseMessage Unauthorized<T>(T response)
        {
            return CreateResponse(HttpStatusCode.Unauthorized, response);
        }
    }
}
