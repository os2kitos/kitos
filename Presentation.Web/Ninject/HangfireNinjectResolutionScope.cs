using System;
using Hangfire;
using Ninject;
using Ninject.Activation.Caching;

namespace Presentation.Web.Ninject
{
    public class HangfireNinjectResolutionScope : JobActivatorScope
    {
        private readonly IKernel _kernel;

        public HangfireNinjectResolutionScope(IKernel kernel)
        {
            _kernel = kernel;
        }

        public override object Resolve(Type type)
        {
            return _kernel.Get(type);
        }

        public override void DisposeScope()
        {
            _kernel.Components.Get<ICache>().Clear(Current);
        }
    }
}