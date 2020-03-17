using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject.Web.Common;
using Presentation.Web;
using Hangfire;
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
            bootstrapper.Initialize(()=> new KernelBuilder().ForWebApplication().Build());

            //Setup for hangfire with a different setup
            GlobalConfiguration.Configuration.UseNinjectActivator(new KernelBuilder().ForHangFire().Build());
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
