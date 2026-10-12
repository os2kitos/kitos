using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.OData;

namespace Presentation.Web.Infrastructure.Configuration
{
    public static class MvcServiceCollectionExtensions
    {
        public static IServiceCollection AddKitosMvc(this IServiceCollection services)
        {
            services
                .AddControllersWithViews(options =>
                {
                    // Block rights-holder-only users from all endpoints by default.
                    // Endpoints that should be accessible to rights holders are decorated with [AllowRightsHoldersAccess].
                    options.Filters.Add(new DenyRightsHoldersAccessAttribute());
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                })
                .AddOData(options =>
                {
                    options.AddRouteComponents("odata", ODataModelConfig.GetEdmModel(), odataServices =>
                    {
                        odataServices.AddSingleton<IFilterBinder, CaseInsensitiveContainsFilterBinder>();
                        odataServices.AddSingleton<ODataUriResolver>(_ => new StringAsEnumResolver { EnableCaseInsensitive = true });
                    });
                    options.EnableQueryFeatures();
                });

            return services;
        }
    }
}
