using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Ninject.Infrastructure.Language;
using Presentation.Web;
using Presentation.Web.Helpers;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.Application.RuntimeEnv;
using Presentation.Web.Swagger;
using Swashbuckle.Application;
using Swashbuckle.OData;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace Presentation.Web
{
    public class SwaggerConfig
    {
        private class ApiVersions
        {
            public const int V1 = 1;
            public const int V2 = 2;
        }

        /// <summary>
        /// Produce RFC3986 compliant ids for model descriptions
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        private static string BuildSchemaId(Type modelType)
        {
            if (!modelType.IsConstructedGenericType) return modelType.Name;

            var prefix = modelType.GetGenericArguments()
                .Select(BuildSchemaId)
                .Aggregate((previous, current) => previous + current);

            return (prefix + modelType.Name.Split('`').First())
                .Replace("[]", "_array_"); //Remove array annotations which produce invalid document ids
        }

        public static void Register()
        {
            GlobalConfiguration.Configuration.EnableSwagger(c =>
                {
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
                    var commentsFile = Path.Combine(baseDirectory, "bin", commentsFileName);
                    c.ApiKey("KITOS TOKEN")
                        .Name("Authorization")
                        .Description("The KITOS TOKEN")
                        .In("header");


                    var swaggerIssueDescription =
                        "<b>KENDTE FEJL OG BEGRÆNSNINGER PÅ DENNE HJÆLPESIDE SAMT WORKAROUND</b><br/>" +
                        "Felter der består af lister af enum værdier vises ikke rigtigt i denne UI. Konkret vises de mulige valg ikke, men i stedet vises 'Array[string]'. For et retvisende billede af dokumentationen anbefales derfor følgende workaround:<br/><br/>" +
                        "- JSON downloades via 'docs linket i toppen'<br/>" +
                        "- Indholdet indsættes i anden editor f.eks. <a href='https://editor.swagger.io' target='_blank'>Swagger.io</a><br/><br/>" +
                        "<b>BEMÆRK</b>: Funktionen 'Try it out' virker p.t. ikke i den eksterne editor." +
                        "";

                    c.MultipleApiVersions((description, version) => version == ApiVersions.V1.ToString() || version == ApiVersions.V2.ToString(), builder =>
                      {
                          //NOTE: Add new versions to the top so that users are always presented with the latest version by default (first one added)
                          builder.Version("2", "OS2Kitos API - V2")
                              .Description(
                                  "<b><i>OBS: Dokumentation for V1 findes ved at skifte version på dokumentet til 1 øverst på siden</i></b><br/><br/>" +
                                  "KITOS API V2 understøtter både læse- og skriveoperationer for de væsentlige registreringsobjekter i KITOS. <br/><br/>" +
                                  "Se mere om designet og konventionerne i API'et her: <a href='https://os2web.atlassian.net/wiki/spaces/KITOS/pages/2059599873/API+Design+V2'>API V2</a>.<br/><br/>" +
                                  "Generelt er anvendelsen af KITOS API(er) beskrevet på projektets <a href='https://os2web.atlassian.net/wiki/spaces/KITOS/pages/658145384/S+dan+kommer+du+igang'>Confluence side</a>.<br/><br/>" +
                                  swaggerIssueDescription
                              );
                          builder.Version("1", "OS2Kitos API - V1")
                              .Description(
                                  "<b><i>OBS: Dokumentation for V2 findes ved at skifte version på dokumentet til 2 øverst på siden</i></b><br/><br/>" +
                                  "<b>BEMÆRK: ADGANG TIL API V1 LUKKES. <a href='https://os2web.atlassian.net/wiki/spaces/KITOS/pages/657293331/API+Design+V1#Varsling-om-lukning'>LÆS MERE HER</a>.</b><br/><br/>" +
                              "Denne dokumentation udstiller Kitos API'et til brug for applikationsudvikling.<br/><br/>" +
                              "Den første udgave af API'et (V1) blev udviklet til understøttelse af projektets brugerflade og vil med tiden blive erstattet " +
                              "af et selvstændigt API (V2) udviklet til brug for integration med udefrakommende systemer. " +
                              "Du vil i en periode kunne anvende både V1 og V2. " +
                              "Bemærk dog, at overflødiggjorte V1 endpoints vil blive udfaset efter en rum tid. KITOS sekretariatet vil i god tid " +
                              "forinden varsle udfasning af overflødige endpoints.<br/><br/>" +
                              "Særligt for V1 gælder der følgende:<br/>" +
                              "ObjectOwnerId, LastChanged og LastChangedByUserId bliver som udgangspunkt sat af systemet automatisk.<br/>" +
                              "Der er udelukkende adgang til læseoperationer i V1. Ved behov for adgang til funktionalitet, der ændrer i data, kontakt da venligst KITOS sekretariatet.<br/><br/>" +
                              "Generelt er anvendelsen beskrevet på projektets <a href='https://os2web.atlassian.net/wiki/spaces/KITOS/pages/658145384/S+dan+kommer+du+igang'>Confluence side</a>.<br/><br/>" +
                                "<b>BEMÆRK: ADGANG TIL API V1 LUKKES. <a href='https://os2web.atlassian.net/wiki/spaces/KITOS/pages/657293331/API+Design+V1#Varsling-om-lukning'>LÆS MERE HER</a>.</b><br/><br/>" +
                                  swaggerIssueDescription
                              );
                      });

                    c.DocumentFilter(() => new FilterByApiVersionFilter(doc => int.Parse(doc.info.version), path => path.IsExternalApiPath() ? ApiVersions.V2 : ApiVersions.V1));

                    var environment = KitosEnvironmentConfiguration.FromConfiguration().Environment;
                    if (environment != KitosEnvironment.Dev)
                    {
                        //Only remove internal api descriptions on the real environments allowing us to use the internal api docs locally and while deployed to dev (for swagger based code gen in frontend)
                        c.DocumentFilter<RemoveInternalApiOperationsFilter>();
                        c.DocumentFilter(() => new RemoveMutatingCallsFilter(doc => int.Parse(doc.info.version) < 2));
                    }
                    c.OperationFilter<CreateOperationIdOperationFilter>();
                    c.OperationFilter<FixNamingOfComplexQueryParametersFilter>();
                    c.OperationFilter<FixContentParameterTypesOnSwaggerSpec>();
                    c.GroupActionsBy(apiDesc =>
                        {
                            var controllerName = apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
                            var suffix = apiDesc.ActionDescriptor.ControllerDescriptor.ControllerType.HasAttribute(typeof(InternalApiAttribute)) ? "[INTERNAL]" : string.Empty;
                            string prefix;
                            if (apiDesc.RelativePath.IsExternalApiPath())
                            {
                                prefix = "API V2 - " + (controllerName.EndsWith("V2", StringComparison.OrdinalIgnoreCase) ? controllerName.Substring(0, controllerName.Length - 2) : controllerName);
                            }
                            else if (apiDesc.RelativePath.Contains("api"))
                            {
                                prefix = "API - V1 - " + controllerName;
                            }
                            else
                            {
                                prefix = "API - V1 (ODATA) - " + controllerName;
                            }
                            return $"{prefix.TrimEnd()} {suffix.Trim()}";
                        }
                    );
                    c.IncludeXmlComments(commentsFile);

                    c.DescribeAllEnumsAsStrings();

                    //Fix invalid names (generics etc)
                    c.SchemaId(BuildSchemaId);

                    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                    //Do not enable the build-in caching in the odata provider. It caches error responses which we dont want so we wrap it in a custom caching provider which bails out on errors
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