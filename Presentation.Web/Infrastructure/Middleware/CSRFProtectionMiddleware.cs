using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using Core.ApplicationServices.Authentication;
using Microsoft.Owin;
using Ninject;
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
            var kernel = context.GetNinjectKernel();
            var authenticationContext = kernel.Get<IAuthenticationContext>();
            if (authenticationContext.Method == AuthenticationMethod.KitosToken)
            {
                await Next.Invoke(context);
                return;
            }

            if (context.Request.Method.IsGet())
            {
                await Next.Invoke(context);
                return;
            }


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
    }
}