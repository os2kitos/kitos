using Microsoft.Owin;
using Owin;
using Hangfire;
[assembly: OwinStartup(typeof(Presentation.Web.Startup))]

namespace Presentation.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            // Initializing the Hangfire scheduler
            GlobalConfiguration.Configuration.UseSqlServerStorage("kitos_HangfireDB");
            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }

    }
}
