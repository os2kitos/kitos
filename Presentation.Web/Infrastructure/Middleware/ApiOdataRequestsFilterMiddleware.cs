using System.Threading.Tasks;
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
            //if (context.Authentication.User.Identity.AuthenticationType != "JWT")
            //{
                await Next.Invoke(context);
            //}
            //else
            //{
            //    if (context.Request.Path.ToString().Contains("odata/"))
            //    {
            //        context.Response.StatusCode = 403;
            //        context.Response.Write("Det er ikke tilladt at kalde odata endpoints");
            //    }
            //    else
            //    {
            //        await Next.Invoke(context);
            //    }
            //}
        }
    }
}