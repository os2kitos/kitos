using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi;

namespace Presentation.Web.Swagger
{
    public static class SwashbuckleExtensions
    {
        public static IEnumerable<OpenApiOperation> EnumerateOperations(this IOpenApiPathItem? pathItem)
        {
            if (pathItem == null || pathItem.Operations == null) yield break;
            foreach (var operation in pathItem.Operations.Values)
                yield return operation;
        }

        public static IEnumerable<IOpenApiSchema> EnumerateSchemas(this OpenApiOperation operation)
        {
            if (operation == null) yield break;

            foreach (var response in operation.Responses ?? new OpenApiResponses())
            {
                foreach (var content in response.Value.Content?.Values ?? Enumerable.Empty<OpenApiMediaType>())
                {
                    if (content.Schema != null) yield return content.Schema;
                    if (content.Schema?.Items != null) yield return content.Schema.Items;
                }
            }

            foreach (var parameter in operation.Parameters ?? new List<IOpenApiParameter>())
            {
                if (parameter.Schema != null) yield return parameter.Schema;
            }

            if (operation.RequestBody != null)
            {
                foreach (var content in operation.RequestBody.Content?.Values ?? Enumerable.Empty<OpenApiMediaType>())
                {
                    if (content.Schema != null) yield return content.Schema;
                }
            }
        }

        public static IEnumerable<IOpenApiSchema> StartSchemaEnumeration(this IOpenApiSchema schema, IDictionary<string, IOpenApiSchema> schemaByTypeName)
        {
            return schema.EnumerateSchemaRecursive(schemaByTypeName, new HashSet<string>());
        }

        private static IEnumerable<IOpenApiSchema> EnumerateSchemaRecursive(this IOpenApiSchema schema, IDictionary<string, IOpenApiSchema> schemaByTypeName, ISet<string> visitedDefinitions, bool isRoot = true)
        {
            var root = schema.GetRootSchemaOrNull();
            var rootId = (root as OpenApiSchemaReference)?.Id;
            if (rootId == null) yield break;

            if (isRoot)
            {
                visitedDefinitions.Add(rootId);
                yield return schema;
            }

            foreach (var referencedSchema in schema.FindReferencedSchemas(schemaByTypeName).ToList())
            {
                var refId = (referencedSchema as OpenApiSchemaReference)?.Id;
                if (refId == null || !visitedDefinitions.Add(refId)) continue;

                yield return referencedSchema;

                foreach (var childSchema in referencedSchema.EnumerateSchemaRecursive(schemaByTypeName, visitedDefinitions, false).ToList())
                {
                    var childRefId = (childSchema.GetRootSchemaOrNull() as OpenApiSchemaReference)?.Id;
                    if (childRefId != null && visitedDefinitions.Add(childRefId))
                        yield return childSchema;
                }
            }
        }

        private static IEnumerable<IOpenApiSchema> FindReferencedSchemas(this IOpenApiSchema schema, IDictionary<string, IOpenApiSchema> schemaByTypeName)
        {
            var key = schema.GetSchemaTypeKey();
            if (string.IsNullOrEmpty(key) || !schemaByTypeName.TryGetValue(key, out var definition))
                return Enumerable.Empty<IOpenApiSchema>();

            if (definition.Properties == null)
                return Enumerable.Empty<IOpenApiSchema>();

            return definition.Properties.Values
                .Select(p => p.GetRootSchemaOrNull())
                .Where(s => s != null)!;
        }

        public static string? GetSchemaRefOrNull(this IOpenApiSchema? schema) =>
            (GetRootSchemaOrNull(schema) as OpenApiSchemaReference)?.Id;

        public static string? GetSchemaTypeKey(this IOpenApiSchema? schema) =>
            GetSchemaRefOrNull(schema);

        public static IOpenApiSchema? GetRootSchemaOrNull(this IOpenApiSchema? schema)
        {
            if (schema is OpenApiSchemaReference) return schema;
            if (schema?.Items is OpenApiSchemaReference) return schema.Items;
            return null;
        }
    }
}
