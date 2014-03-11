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
            var wrap = new ApiReturnModel<Object>
            {
                msg = msg,
                response = null
            };

            return Request.CreateResponse(statusCode, wrap);
        }

        protected HttpResponseMessage CreateResponse(HttpStatusCode statusCode, Exception e)
        {
            var wrap = new ApiReturnModel<Exception>
            {
                msg = e.Message,
                response = e
            };

            return Request.CreateResponse(statusCode, wrap);
        }

        protected HttpResponseMessage CreateResponse<T>(HttpStatusCode statusCode, T response, string msg = "")
            where T: class
        {
            var wrap = new ApiReturnModel<T>
                {
                    msg = msg,
                    response = response
                };

            return Request.CreateResponse(statusCode, wrap);
        }
    }
}
