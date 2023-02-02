using Swashbuckle.Swagger;
using System.Collections.Generic;

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
                yield return parameter.schema;
            }
        }


        public static IEnumerable<Schema> FindAdditionalSchema(this Schema schema, IDictionary<string, Schema> listOfDefinition)
        {
            if (!string.IsNullOrEmpty(schema.@ref))
            {
                if (listOfDefinition.TryGetValue(schema.@ref.Replace("#/definitions/", string.Empty), out var definition))
                {
                    foreach (var propertySchema in definition.properties)
                    {
                        yield return propertySchema.Value;
                    }
                }
            }
            if (!string.IsNullOrEmpty(schema.items?.@ref))
            {
                if (listOfDefinition.TryGetValue(schema.items.@ref.Replace("#/definitions/", string.Empty), out var definition))
                {
                    foreach (var propertySchema in definition.properties)
                    {
                        yield return propertySchema.Value;
                    }
                }
            }
        }

        public static IEnumerable<Schema> EnumerateSchema(this Schema schema, IDictionary<string, Schema> listOfDefinition, List<string> visitedDefinitions, int dept = 0)
        {
            if (schema?.@ref == null)
            {
                yield break;
            }
            if (visitedDefinitions.Contains(schema.@ref)) //if definition was already visited break to avoid possible circular dependency
            {
                yield break;
            }
            if (dept == 0)
            {
                yield return schema;
            }

            var listOfAdditionalSchema = schema.FindAdditionalSchema(listOfDefinition) ?? new List<Schema>();
            foreach (var additionalSchema in listOfAdditionalSchema)
            {
                yield return additionalSchema;
                foreach (var childSchema in additionalSchema.EnumerateSchema(listOfDefinition, visitedDefinitions, dept + 1) ?? new List<Schema>())
                {
                    yield return childSchema;
                    visitedDefinitions.Add(additionalSchema.@ref);
                }
                visitedDefinitions.Add(additionalSchema.@ref);
            }
        }
    }
}