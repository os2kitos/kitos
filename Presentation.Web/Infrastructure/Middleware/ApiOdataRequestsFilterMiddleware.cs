using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class ApiOdataRequestsFilterMiddleware : OwinMiddleware
    {
        public ApiOdataRequestsFilterMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (context.Authentication.User.Identity.AuthenticationType != "JWT")
            {
                await Next.Invoke(context);
            }
            else
            {
                if (context.Request.Path.ToString().Contains("odata/"))
                {
                    context.Response.StatusCode = 403;
                    context.Response.WriteAsync("Calling odata functions is forbidden");
                }
                else
                {
                    await Next.Invoke(context);
                }
            }
            
        }
    }
}