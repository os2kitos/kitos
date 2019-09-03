using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Presentation.Web;
using Presentation.Web.Helpers;
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
                    .Description("Denne dokumentation udstiller de forskellige kald der kan laves til api'et i kitos. \n" +
                                 "Mange kald bliver oprettet gennem en generisk kontroller, og disse vil ikke blive beskrevet individuelt, men blive påskrevet en værdi fra denne generiske kontroller. \n \n" +
                                 "Til information er det ikke alle parametre der skal bruges når API'et tilgås ObjectOwnerId, LastChanged og LastChangedByUserId bliver som udgangspunkt sat af systemet automatisk. \n \n" +
                                 "I første version af APIet er der udelukkende adgang til læseoperationer. Ved behov for adgang til funktionalitet, der ændrer i data, kontakt da venligst KITOS sekretariatet.");
                c.DocumentFilter<InternalApiAttributeFilter>();

                c.GroupActionsBy(apiDesc =>
                        {
                            if (apiDesc.RelativePath.Contains("api"))
                            {
                                return "API - " + apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
                            }
                            return "ODATA - " + apiDesc.ActionDescriptor.ControllerDescriptor.ControllerName;
                        }
                    );


                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".XML";
                var commentsFile = Path.Combine(baseDirectory, "bin", commentsFileName);
                c.IncludeXmlComments(commentsFile);

                c.DescribeAllEnumsAsStrings();

                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                c.CustomProvider(defaultProvider => new ODataSwaggerProvider(defaultProvider, c, GlobalConfiguration.Configuration).Configure(odataConfig =>
                {
                    odataConfig.EnableSwaggerRequestCaching();
                }));
                

            })
                .EnableSwaggerUi(c =>
                {
                    c.InjectJavaScript(Assembly.GetExecutingAssembly(), "Presentation.Web.Scripts.SwaggerUICustom.js");
                    c.EnableApiKeySupport("Authorization", "header");
                });
            
        }
    }
}