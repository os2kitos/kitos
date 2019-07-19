using System.Threading.Tasks;
using Microsoft.Owin;
using Ninject;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Factories.Authentication;

namespace Presentation.Web.Infrastructure.Middleware
{
    public class KitosContextMiddleware : OwinMiddleware
    {
        public KitosContextMiddleware(OwinMiddleware next) 
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var kernel = context.GetNinjectKernel();

            var authenticationContext = kernel.Get<IAuthenticationContextFactory>().CreateFrom(context);
            
            context.WithEnvironmentProperty(authenticationContext);

            await Next.Invoke(context);
        }
    }
}