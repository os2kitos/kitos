using System.Threading.Tasks;
using Microsoft.Owin;
using Ninject;
using Presentation.Web.Infrastructure.Model.Authentication;
using Serilog;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class DenyUsersWithoutApiAccessMiddleware : OwinMiddleware
    {
        public DenyUsersWithoutApiAccessMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var kernel = context.GetNinjectKernel();
            var logger = kernel.Get<ILogger>();
            var authenticationContext = kernel.Get<IAuthenticationContext>();
            if (authenticationContext.Method == AuthenticationMethod.KitosToken && !authenticationContext.HasApiAccess)
            {
                logger.Warning("User with id: {userID} made an API call without having API access",
                    authenticationContext.UserId);
                context.Response.StatusCode = 403;
                context.Response.Write("Du har ikke tilladelse til at kalde API endpoints");
            }
            else
            {
                await Next.Invoke(context);
            }
        }
    }
}