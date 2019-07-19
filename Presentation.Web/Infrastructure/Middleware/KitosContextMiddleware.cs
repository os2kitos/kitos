using System.Threading.Tasks;
using Microsoft.Owin;
using Ninject;
using Ninject.Web.Common;
using Presentation.Web.Infrastructure.Factories.Authentication;
using Presentation.Web.Infrastructure.Model.Authentication;

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
            kernel.Bind<IAuthenticationContext>().ToConstant(authenticationContext).InRequestScope();

            await Next.Invoke(context);
        }
    }
}