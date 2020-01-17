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
        private const string XsrfCookie = "XSRF-TOKEN";

        public override async Task Invoke(IOwinContext context)
        {
            if (context.Request.Method.IsMutation())
            {
                var headers = context.Request.Headers;

                if (!headers.TryGetValue(XsrfHeader, out var xsrfToken))
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("Manglende xsrf header");
                    return;
                }

                var tokenHeaderValue = xsrfToken.First();
                
                var tokenCookie = context.Request.Cookies.FirstOrDefault(c => c.Key == XsrfCookie);

                if (tokenCookie.Value == null)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("Manglende xsrf cookie");
                    return;
                }

                try
                {
                    AntiForgery.Validate(tokenCookie.Value, tokenHeaderValue);
                    await Next.Invoke(context);
                }
                catch (HttpAntiForgeryException e)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write(e.Message);
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