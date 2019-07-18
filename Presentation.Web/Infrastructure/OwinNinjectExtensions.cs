using Microsoft.Owin;
using Ninject;
using Ninject.Web.Common;
using Owin;

namespace Presentation.Web.Infrastructure
{
    public static class OwinNinjectExtensions
    {
        private const string NinjectKernel = nameof(OwinNinjectExtensions) + nameof(NinjectKernel);

        public static IAppBuilder UseCustomScopeForRequest(this IAppBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                //Add Ninject to the following middlewares
                AddNinjectKernel(context);

                await next();
            });
        }

        private static void AddNinjectKernel(IOwinContext context)
        {
            //Bootstrapper holds a singleton reference to the root kernel accessed through an instance property.
            context.Environment[NinjectKernel] = new Bootstrapper().Kernel;
        }

        public static IKernel GetNinjectKernel(this IOwinContext context)
        {
            return (IKernel)context.Environment[NinjectKernel];
        }

    }
}