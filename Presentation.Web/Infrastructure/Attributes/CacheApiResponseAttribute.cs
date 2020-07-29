using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class CacheApiResponseAttribute : ActionFilterAttribute
    {
        public int DurationInMiliSeconds { get; set; } = 1000;

        public override void OnActionExecuted(HttpActionExecutedContext filterContext)
        {
            filterContext.Response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = TimeSpan.FromMilliseconds(DurationInMiliSeconds),
                MustRevalidate = true,
                Private = true
            };
            filterContext.Response.Headers.Vary.Add("Cookie");
        }
    }
}