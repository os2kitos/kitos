using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using Core.ApplicationServices.Authentication;
using Microsoft.Owin;
using Ninject;
using Presentation.Web.Extensions;
using Presentation.Web.Helpers;
using Serilog;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class CSRFProtectionMiddleware : OwinMiddleware
    {
        public CSRFProtectionMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var kernel = context.GetNinjectKernel();
            var logger = kernel.Get<ILogger>();
            var authenticationContext = kernel.Get<IAuthenticationContext>();
            if (authenticationContext.Method == AuthenticationMethod.KitosToken)
            {
                await Next.Invoke(context);
                return;
            }

            if (!context.Request.Method.IsMutation())
            {
                await Next.Invoke(context);
                return;
            }

            var headers = context.Request.Headers;
            if (!headers.TryGetValue(Constants.CSRFValues.HeaderName, out var xsrfToken))
            {
                context.Response.StatusCode = 400;
                context.Response.Write(Constants.CSRFValues.MissingXsrfHeaderError);
                return;
            }

            var tokenHeaderValue = xsrfToken.First();
            var tokenCookie = context.Request.Cookies.FirstOrDefault(c => c.Key == Constants.CSRFValues.CookieName);
            if (tokenCookie.Value == null)
            {
                context.Response.StatusCode = 400;
                context.Response.Write(Constants.CSRFValues.MissingXsrfCookieError);
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
                logger.Error(e.Message);
                context.Response.Write(Constants.CSRFValues.XsrfValidationFailedError);
            }


        }
    }
}