using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Presentation.Web;
using Presentation.Web.Helpers;
using Presentation.Web.Swagger;
using Swashbuckle.Application;
using Swashbuckle.OData;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Presentation.Web
{
    public class SwaggerConfig
    {
        private class ApiVersions
        {
            public const int V1 = 1;
            public const int V2 = 2;
        }
        public static void Register()
        {
            GlobalConfiguration.Configuration.EnableSwagger(c =>
                {
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
                    var commentsFile = Path.Combine(baseDirectory, "bin", commentsFileName);

                    c.MultipleApiVersions((description, version) => version == ApiVersions.V1.ToString() || version == ApiVersions.V2.ToString(), builder =>
                      {
                          //NOTE: Add new versions to the top so that users are always presented with the latest version by default (first one added)
                          builder.Version("2", "OS2Kitos API - V2")
                              .Description(
                                  "<b><i>OBS: Dokumentation for V1 findes ved at skifte version på dokumentet til 1 øverst på siden</i></b><br/><br/>" +
                                  "Arbejdet med V2 er påbegyndt og " +
                                  "resultatet heraf opdateres løbende på denne side. I første omgang vil V2 omfatte supplerende data fra it-system- og " +
                                  "snitfladekataloget. V2 kommer til at omfatte tilsvarende funktionalitet som V1.<br/><br/>" +
                                  "Generelt er anvendelsen beskrevet på projektets <a href='https://os2web.atlassian.net/wiki/spaces/KITOS/pages/658145384/S+dan+kommer+du+igang'>Confluence side</a>.<br/>"
                              );
                          builder.Version("1", "OS2Kitos API - V1")
                              .Description(
                                  "<b><i>OBS: Dokumentation for V2 findes ved at skifte version på dokumentet til 2 øverst på siden</i></b><br/><br/>" +
                              "<br/>Denne dokumentation udstiller Kitos API'et til brug for applikationsudvikling.<br/><br/>" +
                              "Den første udgave af API'et (V1) blev udviklet til understøttelse af projektets brugerflade og vil med tiden blive erstattet " +
                              "af et selvstændigt API (V2) udviklet til brug for integration med udefrakommende systemer. " +
                              "Du vil i en periode kunne anvende både V1 og V2. " +
                              "Bemærk dog, at overflødiggjorte V1 endpoints vil blive udfaset efter en rum tid. KITOS sekretariatet vil i god tid " +
                              "forinden varsle udfasning af overflødige endpoints.<br/><br/>" +
                              "Særligt for V1 gælder der følgende:<br/>" +
                              "ObjectOwnerId, LastChanged og LastChangedByUserId bliver som udgangspunkt sat af systemet automatisk.<br/>" +
                              "Der er udelukkende adgang til læseoperationer i V1. Ved behov for adgang til funktionalitet, der ændrer i data, kontakt da venligst KITOS sekretariatet.<br/><br/>" +
                              "Generelt er anvendelsen beskrevet på projektets <a href='https://os2web.atlassian.net/wiki/spaces/KITOS/pages/658145384/S+dan+kommer+du+igang'>Confluence side</a>.<br/>"
                              );
                      });

                    c.DocumentFilter(() => new FilterByApiVersionFilter(doc => int.Parse(doc.info.version), path => path.IsExternalApiPath() ? ApiVersions.V2 : ApiVersions.V1));
                    c.DocumentFilter<RemoveInternalApiOperationsFilter>();
                    c.DocumentFilter(() => new RemoveMutatingCallsFilter(doc => int.Parse(doc.info.version) < 2));
                    c.OperationFilter<FixNamingOfComplexQueryParametersFilter>();
                    c.GroupActionsBy(apiDesc =>
                        {
                            var controllerName = apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
                            if (apiDesc.RelativePath.IsExternalApiPath())
                                return "API V2 - " + (controllerName.EndsWith("V2",StringComparison.OrdinalIgnoreCase) ? controllerName.Substring(0,controllerName.Length-2) : controllerName);
                            if (apiDesc.RelativePath.Contains("api"))
                                return "API - V1 - " + controllerName;
                            return "API - V1 (ODATA) - " + controllerName;
                        }
                    );
                    c.IncludeXmlComments(commentsFile);

                    c.DescribeAllEnumsAsStrings();

                    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                    //Do not enable the build in caching in the odata provider. It caches error responses which we dont want so we wrap it in a custom caching provider which bails out on errors
                    c.CustomProvider(defaultProvider => new CustomCachingSwaggerProvider(new ODataSwaggerProvider(defaultProvider, c, GlobalConfiguration.Configuration)));
                })
                .EnableSwaggerUi(c =>
                {
                    c.InjectJavaScript(Assembly.GetExecutingAssembly(), "Presentation.Web.Scripts.SwaggerUICustom.js");
                    c.EnableApiKeySupport("Authorization", "header");
                });

        }
    }
}