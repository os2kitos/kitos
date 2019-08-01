﻿using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class InternalApiAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Contains("Authorization"))
            {
                actionContext.Response = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Content = new StringContent("Det er ikke tilladt at benytte dette endpoint")
                };

            }
            base.OnActionExecuting(actionContext);


        }

    }
}
