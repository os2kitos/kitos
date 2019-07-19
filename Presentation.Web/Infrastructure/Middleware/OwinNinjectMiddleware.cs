using Microsoft.Owin;
using Ninject;
using Ninject.Web.Common;
using Owin;
using Presentation.Web.Helpers;

namespace Presentation.Web.Infrastructure.Middleware
{
    public static class OwinNinjectMiddleware
    {
        public static IAppBuilder UseNinject(this IAppBuilder app)
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
            context.WithEnvironmentProperty(new Bootstrapper().Kernel);
        }

        public static IKernel GetNinjectKernel(this IOwinContext context)
        {
            return context.GetEnvironmentProperty<IKernel>();
        }

    }
}