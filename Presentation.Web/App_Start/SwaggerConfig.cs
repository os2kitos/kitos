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
        public static void Register()
        {
            GlobalConfiguration.Configuration.EnableSwagger(c =>
                {
                    c.SingleApiVersion("1.0.0", "OS2Kitos API")
                        .Description(
                            "<br>Denne dokumentation udstiller Kitos API'et til brug for applikationsudvikling.<br><br>" +
                            "Den første udgave af API'et (V1) blev udviklet til understøttelse af projektets brugerflade og vil med tiden blive erstattet " +
                            "af et selvstændigt API (V2) udviklet til brug for integration med udefrakommende systemer. Arbejdet med V2 er påbegyndt og " +
                            "resultatet heraf opdateres løbende på denne side. I første omgang vil V2 omfatte supplerende data fra it-system- og " +
                            "snitfladekataloget. V2 kommer til at omfatte tilsvarende funktionalitet som V1. Du vil i en periode kunne anvende både V1 og V2. " +
                            "Bemærk dog, at overflødiggjorte V1 endpoints vil blive udfaset efter en rum tid. KITOS sekretariatet vil i god tid " +
                            "forinden varsle udfasning af overflødige endpoints.<br><br>" +
                            "Særligt for V1 gælder der følgende:<br>" +
                            "ObjectOwnerId, LastChanged og LastChangedByUserId bliver som udgangspunkt sat af systemet automatisk.<br>" +
                            "Der er udelukkende adgang til læseoperationer. Ved behov for adgang til funktionalitet, der ændrer i data, kontakt da venligst KITOS sekretariatet.<br><br>" +
                            "Generelt er anvendelsen beskrevet på projektets <a href='https://os2web.atlassian.net/wiki/spaces/KITOS/pages/658145384/S+dan+kommer+du+igang'>Confluence side</a>.<br>");

                    c.DocumentFilter<RemoveInternalApiOperationsFilter>();
                    c.DocumentFilter<RemoveMutatingCallsFilter>();
                    c.GroupActionsBy(apiDesc =>
                        {
                            if (apiDesc.RelativePath.IsExternalApiPath())
                                return "API V2 - " + apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
                            if (apiDesc.RelativePath.Contains("api"))
                                return "API - " + apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
                            return "API - ODATA - " + apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
                        }
                    );

                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
                    var commentsFile = Path.Combine(baseDirectory, "bin", commentsFileName);
                    c.IncludeXmlComments(commentsFile);

                    c.DescribeAllEnumsAsStrings();

                    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                    c.CustomProvider(defaultProvider =>
                        new ODataSwaggerProvider(defaultProvider, c, GlobalConfiguration.Configuration).Configure(
                            odataConfig => { odataConfig.EnableSwaggerRequestCaching(); }));
                })
                .EnableSwaggerUi(c =>
                {
                    c.InjectJavaScript(Assembly.GetExecutingAssembly(), "Presentation.Web.Scripts.SwaggerUICustom.js");
                    c.EnableApiKeySupport("Authorization", "header");
                });
        }
    }
}