using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.Owin;
using Presentation.Web.Extensions;

namespace Presentation.Web.Infrastructure.Middleware 
{
    public class CSRFProtectionMiddleware : OwinMiddleware
    {
        public CSRFProtectionMiddleware(OwinMiddleware next) : base(next)
        {
        }

        private const string XsrfHeader = "X-XSRF-TOKEN";
        private const string XsrfCookie = "__RequestVerificationToken";

        public override async Task Invoke(IOwinContext context)
        {
            if (context.Request.Method.IsMutation())
            {
                var headers = context.Request.Headers;

                if (!headers.TryGetValue(XsrfHeader, out var xsrfToken))
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("Manglende xsrf token");
                    return;
                }

                var tokenHeaderValue = xsrfToken.First();
                
                var tokenCookie = context.Request.Cookies.Single(c => c.Key == XsrfCookie).Value;

                if (tokenCookie == null)
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                try
                {
                    AntiForgery.Validate(tokenCookie, tokenHeaderValue);
                    await Next.Invoke(context);
                }
                catch (HttpAntiForgeryException)
                {
                    context.Response.StatusCode = 400;
                }
                
            }
            else
            {
                await Next.Invoke(context);
            }












            //if (IsSameOrigin(context.Request.Headers))
            //{
            //    await Next.Invoke(context);
            //}

            //context.Response.StatusCode = 403;
            //context.Response.Write("CSRF beskyttet");
        }





        //private static bool IsSameOrigin(IHeaderDictionary headers)
        //{
        //    if (!headers.ContainsKey("origin") || !headers.ContainsKey("Referer"))
        //    {
        //        return false;
        //    }

        //    var origin = headers.GetValues("origin").First();
        //    var referer = headers.GetValues("Referer").First();
        //    if (origin.IsEmpty() || referer.IsEmpty())
        //    {
        //        return false;
        //    }
        //    return Equals(origin, referer);
        //}
    }
}