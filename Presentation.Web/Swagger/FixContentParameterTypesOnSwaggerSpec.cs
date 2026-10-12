using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;
using Presentation.Web.Helpers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Presentation.Web.Swagger
{
    public class FixContentParameterTypesOnSwaggerSpec : IOperationFilter
    {
        private const string Json = "application/json";
        private const string JsonMergePatch = "application/merge-patch+json"; // https://datatracker.ietf.org/doc/html/rfc7396

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Remove merge-patch from request body content
            operation.RequestBody?.Content?.Remove(JsonMergePatch);

            // Remove merge-patch from all response content
            var responses = operation.Responses ?? [];
            foreach (var response in responses.Values)
            {
                response.Content?.Remove(JsonMergePatch);
            }

            var relativePath = context.ApiDescription.RelativePath ?? string.Empty;
            if (!relativePath.IsExternalApiPath())
                return;

            var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? string.Empty;

            // Save the body schema before clearing so it can be re-attached after.
            // Swashbuckle generates the correct schema from the [FromBody] parameter; we must not lose it.
            IOpenApiSchema? existingBodySchema = null;
            var existingResponseSchemas = responses
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp =>
                    {
                        OpenApiMediaType? existingJsonContent = null;
                        kvp.Value.Content?.TryGetValue(Json, out existingJsonContent);
                        return existingJsonContent?.Schema;
                    });

            if (httpMethod != "DELETE")
            {
                OpenApiMediaType? existingJsonContent = null;
                operation.RequestBody?.Content?.TryGetValue(Json, out existingJsonContent);
                existingBodySchema = existingJsonContent?.Schema;

                if (operation.RequestBody != null)
                    operation.RequestBody.Content?.Clear();
            }

            foreach (var response in responses.Values)
                response.Content?.Clear();

            switch (httpMethod)
            {
                case "POST":
                case "PUT":
                    EnsureRequestBodyContent(operation, Json, existingBodySchema);
                    EnsureResponseContent(operation, Json, existingResponseSchemas);
                    break;
                case "PATCH":
                    EnsureRequestBodyContent(operation, JsonMergePatch, existingBodySchema);
                    EnsureRequestBodyContent(operation, Json, existingBodySchema);
                    EnsureResponseContent(operation, Json, existingResponseSchemas);
                    break;
                case "GET":
                    EnsureResponseContent(operation, Json, existingResponseSchemas);
                    break;
                default:
                    break;
            }
        }

        private static void EnsureRequestBodyContent(OpenApiOperation operation, string mediaType, IOpenApiSchema? schema = null)
        {
            // Only add content to an existing request body; never fabricate one.
            // Endpoints without a [FromBody] parameter have no request body and must not get one injected.
            if (operation.RequestBody == null) return;

            operation.RequestBody.Content?.TryAdd(mediaType, new OpenApiMediaType { Schema = schema });
        }

        private static void EnsureResponseContent(OpenApiOperation operation, string mediaType, IDictionary<string, IOpenApiSchema?> existingResponseSchemas)
        {
            foreach (var response in operation.Responses ?? [])
            {
                if (response.Value.Content == null) continue;

                existingResponseSchemas.TryGetValue(response.Key, out var schema);
                response.Value.Content.TryAdd(mediaType, new OpenApiMediaType { Schema = schema });
            }
        }
    }
}
