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
                    Response = null
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
            where T : class
        {
            var wrap = new ApiReturnDTO<T>
                {
                    Msg = msg,
                    Response = response
                };

            return Request.CreateResponse(statusCode, wrap);
        }
    }
}
