using System.Threading.Tasks;
using Core.ApplicationServices.Authentication;
using Microsoft.Owin;
using Ninject;
using Presentation.Web.Extensions;
using Serilog;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class DenyModificationsThroughApiMiddleware : OwinMiddleware
    {
        public DenyModificationsThroughApiMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var kernel = context.GetNinjectKernel();
            var logger = kernel.Get<ILogger>();
            var authenticationContext = kernel.Get<IAuthenticationContext>();
            if (authenticationContext.Method == AuthenticationMethod.KitosToken && IsMutationAttempt(context))
            {
                logger.Warning("User with id: {userID} attempted to mutate resource: {url} by method {method}",
                    authenticationContext.UserId, context.Request.Uri.ToString(), context.Request.Method);
                context.Response.StatusCode = 403;
                context.Response.Write("Det er ikke tilladt at skrive data via APIet");
            }
            else
            {
                await Next.Invoke(context);
            }
        }

        private static bool IsMutationAttempt(IOwinContext context)
        {
            return context.Request.Method.IsMutation();
        }
    }
}