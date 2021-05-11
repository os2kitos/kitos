using System.Threading.Tasks;
using Core.ApplicationServices.Authentication;
using Microsoft.Owin;
using Ninject;
using Presentation.Web.Extensions;
using Presentation.Web.Helpers;
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
            if (IsKitosTokenAuthenticated(authenticationContext) && IsIllegalMutationAttempt(context))
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

        private static bool IsKitosTokenAuthenticated(IAuthenticationContext authenticationContext)
        {
            return authenticationContext.Method == AuthenticationMethod.KitosToken;
        }

        private static bool IsIllegalMutationAttempt(IOwinContext context)
        {
            return (context.Request.Method.IsMutation() && IsNotExternalApiUsage(context));
        }

        private static bool IsNotExternalApiUsage(IOwinContext context)
        {
            return !context.Request.Uri.AbsoluteUri.IsExternalApiPath();
        }
    }
}