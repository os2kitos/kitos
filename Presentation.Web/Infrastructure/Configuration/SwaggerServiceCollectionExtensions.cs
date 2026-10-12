using Infrastructure.Services.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Presentation.Web.Controllers.API.V1.Auth;
using Presentation.Web.Helpers;
using Presentation.Web.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Web.Infrastructure.Configuration
{
    public static class SwaggerServiceCollectionExtensions
    {
        public static IServiceCollection AddKitosSwagger(this IServiceCollection services, bool isDevelopment)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "KITOS API V1", Version = "1" });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "KITOS API V2", Version = "2" });

                var xmlFile = $"{typeof(SwaggerServiceCollectionExtensions).Assembly.GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    // Skip actions without an explicit HTTP method binding (OData, naming-convention V1, etc.)
                    if (string.IsNullOrEmpty(apiDesc.HttpMethod)) return false;

                    var isV2Path = (apiDesc.RelativePath ?? "").IsExternalApiPath();

                    if (docName == "v2") return isV2Path;

                    // V1 doc: in development show all V1 endpoints; in release only token authentication
                    if (isV2Path) return false;
                    if (isDevelopment) return true;
                    return apiDesc.ActionDescriptor is ControllerActionDescriptor cad &&
                           cad.ControllerTypeInfo == typeof(TokenAuthenticationController);
                });

                c.CustomSchemaIds(type =>
                {
                    if (!type.IsConstructedGenericType) return type.Name;
                    string Recurse(Type t) => !t.IsConstructedGenericType
                        ? t.Name
                        : t.GetGenericArguments().Select(Recurse).Aggregate((a, b) => a + b) + t.Name.Split('`')[0];
                    return Recurse(type).Replace("[]", "_array_");
                });

                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                c.AddSecurityDefinition(AuthenticationSchemes.Bearer, new OpenApiSecurityScheme
                {
                    Name = AuthenticationSchemes.Bearer,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter the KITOS token obtained from the GetToken endpoint (without the 'Bearer ' prefix)."
                });

                // Apply the Bearer scheme globally so Swagger UI sends the Authorization header on every request.
                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    { new OpenApiSecuritySchemeReference(AuthenticationSchemes.Bearer, document), new List<string>() }
                });

                c.DocumentFilter<FilterByApiVersionFilter>(
                    (Func<OpenApiDocument, int>)(doc => int.TryParse(doc.Info?.Version, out var v) ? v : 1),
                    (Func<string, int>)(path => path.IsExternalApiPath() ? 2 : 1));
                c.DocumentFilter<RemoveUnneededMutatingCallsFilter>(
                    (Predicate<OpenApiDocument>)(doc => int.TryParse(doc.Info?.Version, out var v) && v < 2));
                if (!isDevelopment)
                    c.DocumentFilter<RemoveInternalApiOperationsFilter>();
                c.DocumentFilter<OnlyIncludeReadModelSchemasInSwaggerDocumentFilter>();
                c.DocumentFilter<PurgeUnusedTypesDocumentFilter>(
                    (Predicate<OpenApiDocument>)(doc => int.TryParse(doc.Info?.Version, out var v) && v >= 2));

                // Register all DTO model types so they appear in the Swagger "Models" section.
                // Applied after PurgeUnusedTypesDocumentFilter to ensure the manually-added schemas survive the purge.
                // V1 models are only relevant in development since non-dev environments restrict V1 to token auth only.
                c.DocumentFilter<RegisterDtoSchemasDocumentFilter>(
                    (Predicate<OpenApiDocument>)(doc => isDevelopment && int.TryParse(doc.Info?.Version, out var v) && v < 2),
                    "Presentation.Web.Models.API.V1");
                c.DocumentFilter<RegisterDtoSchemasDocumentFilter>(
                    (Predicate<OpenApiDocument>)(doc => int.TryParse(doc.Info?.Version, out var v) && v >= 2),
                    "Presentation.Web.Models.API.V2");

                c.OperationFilter<AddBearerSecurityRequirementOperationFilter>();
                c.OperationFilter<CreateOperationIdOperationFilter>();
                c.OperationFilter<FixNamingOfComplexQueryParametersFilter>();
                c.OperationFilter<FixContentParameterTypesOnSwaggerSpec>();

                c.SchemaFilter<SupplierFieldSchemaFilter>();
                c.SchemaFilter<StringEnumSchemaFilter>();
            });

            return services;
        }
    }
}
