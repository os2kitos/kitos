using Microsoft.Owin;
using Ninject.Activation.Blocks;
using Ninject.Web.Common;
using Owin;

namespace OwinNinjectExample.Ninject
{
    public static class OwinNinjectExtensions
    {
        private const string ActivationBlock = nameof(OwinNinjectExtensions) + nameof(ActivationBlock);

        public static IAppBuilder UseCustomScopeForRequest(this IAppBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                //Create a shared activationblock (for the next middleware in the chain) and add it to context
                using (WithNewResolutionScope(context))
                {
                    await next();
                }
            });
        }

        private static IActivationBlock WithNewResolutionScope(IOwinContext context)
        {
            //Bootstrapper holds a singleton reference to the root kernel accessed through an instance property.
            var activationBlock = new Bootstrapper().Kernel.BeginBlock();
            context.Environment[ActivationBlock] = activationBlock;
            return activationBlock;
        }

        public static IActivationBlock GetResolutionScope(this IOwinContext context)
        {
            return (IActivationBlock)context.Environment[ActivationBlock];
        }

    }
}