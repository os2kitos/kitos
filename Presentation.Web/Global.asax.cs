using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Presentation.Web.Infrastructure.Config;
using Presentation.Web.Ninject;

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
            AuthConfig.RegisterAuth();

            // Turns off self reference looping when serializing models in API controlllers
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // Support polymorphism in web api JSON output
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;

            // Set JSON serialization in WEB API to use camelCase (javascript) instead of PascalCase (C#)
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Convert all dates to UTC
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            // Add merge-patch+json as accepted media type
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/merge-patch+json"));

            //Configure the V2 json formatter (uses string enum serialization to align with exposed docs)
            V2JsonSerializationConfig.Configure(GlobalConfiguration.Configuration.Formatters.JsonFormatter);

            //Add to MVC pipeline
            ControllerBuilder.Current.SetControllerFactory(new DefaultControllerFactory(new NinjectControllerActivator()));
        }
    }
}
