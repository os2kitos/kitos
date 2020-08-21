using System.Data.Entity.Infrastructure.Interception;
using System.Web.Http;
using Core.DomainServices.Context;
using Core.DomainServices.Time;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject.Web.Common;
using Presentation.Web;
using Infrastructure.DataAccess.Interceptors;
using Infrastructure.Services.Types;
using Ninject;
using Ninject.Web.Common.WebHost;
using Ninject.Web.WebApi;
using Presentation.Web.Ninject;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace Presentation.Web
{
    public static class NinjectWebCommon
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));

            //Setup for web application
            bootstrapper.Initialize(() =>
            {
                var kernel = new KernelBuilder().ForWebApplication().Build();

                //Only register the interceptor once per application (Hangfire job is the same application scope)
                DbInterception.Add(new EFEntityInterceptor(() => kernel.Get<IOperationClock>(), () => kernel.Get<Maybe<ActiveUserIdContext>>(), () => kernel.Get<IFallbackUserResolver>()));

                return kernel;
            });
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
    }
}
