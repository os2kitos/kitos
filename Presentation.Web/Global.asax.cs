using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ninject.Web.Common;
using Ninject.Web.WebApi;

namespace Presentation.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode,
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            LogConfig.RegisterLog();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();

            // Turns off self reference looping when serializing models in API controlllers
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // Support polymorphism in web api JSON output
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;

            // Set JSON serialization in WEB API to use camelCase (javascript) instead of PascalCase (C#)
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Convert all dates to UTC
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            //Add to MVC pipeline
            //TODO: Make dependency injection work with MVC
        }
    }
}
