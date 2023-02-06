using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Web.Swagger
{
    public static class SwashbuckleExtensions
    {
        public static IEnumerable<Operation> EnumerateOperations(this PathItem pathItem)
        {
            if (pathItem == null)
            {
                yield break;
            }
            yield return pathItem.get;
            yield return pathItem.post;
            yield return pathItem.put;
            yield return pathItem.patch;
            yield return pathItem.delete;
            yield return pathItem.options;
            yield return pathItem.head;
        }

        public static IEnumerable<Schema> EnumerateSchema(this Operation operation)
        {
            if (operation == null)
            {
                yield break;
            }
            //Response schemas
            foreach (var response in operation.responses ?? new Dictionary<string, Response>())
            {
                yield return response.Value.schema;
                if (response.Value.schema?.items != null)
                {
                    yield return response.Value.schema.items;
                }
            }
            //Parameter schemas
            foreach (var parameter in operation.parameters ?? new List<Parameter>())
            {
                if (parameter.schema != null)
                {
                    yield return parameter.schema;
                }
            }
        }

        public static IEnumerable<Schema> StartSchemaEnumeration(this Schema schema, IDictionary<string, Schema> schemaByTypeName)
        {
            return schema.EnumerateSchema(schemaByTypeName, new HashSet<string>());
        }

        private static IEnumerable<Schema> EnumerateSchema(this Schema schema, IDictionary<string, Schema> schemaByTypeName, ISet<string> visitedDefinitions, bool isRoot = true)
        {
            if (schema?.@ref == null)
            {
                yield break;
            }

            if (isRoot)
            {
                visitedDefinitions.Add(schema.@ref);
                yield return schema;
            }

            var referencedSchemas = (schema.FindReferencedSchemas(schemaByTypeName) ?? new List<Schema>()).ToList();
            foreach (var additionalSchema in referencedSchemas)
            {
                if (visitedDefinitions.Add(additionalSchema.@ref))
                {
                    yield return additionalSchema;
                    foreach (var childSchema in (additionalSchema.EnumerateSchema(schemaByTypeName, visitedDefinitions.ToHashSet(), false) ?? new List<Schema>()).ToList())
                    {
                        if (visitedDefinitions.Add(childSchema.@ref))
                        {
                            yield return childSchema;
                        }
                    }
                }
            }
        }

        private static IEnumerable<Schema> FindReferencedSchemas(this Schema schema, IDictionary<string, Schema> schemaByTypeName)
        {
            var key = schema.GetSchemaTypeKey();

            if (!string.IsNullOrEmpty(key))
            {
                if (schemaByTypeName.TryGetValue(key, out var definition))
                {
                    var referencedRootSchemas = definition
                        .properties
                        .Values
                        .Select(GetRootSchemaOrNull)
                        .Where(x => x != null)
                        .ToList();

                    foreach (var propertySchema in referencedRootSchemas)
                    {
                        yield return propertySchema;
                    }
                }
            }
        }

        public static string GetSchemaRefOrNull(this Schema schema)
        {
            return GetRootSchemaOrNull(schema)?.@ref;
        }

        public static string GetSchemaTypeKey(this Schema schema)
        {
            return GetSchemaRefOrNull(schema)?.Replace("#/definitions/", string.Empty);
        }

        public static Schema GetRootSchemaOrNull(this Schema schema)
        {
            if (schema.@ref != null)
                return schema;
            if (schema.items?.@ref != null)
                return schema.items;
            return null;
        }
    }
}