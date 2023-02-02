using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    public class FilterByApiVersionFilter : IDocumentFilter
    {
        private readonly Func<SwaggerDocument, int> _getApiVersion;
        private readonly Func<string, int> _getPathApiVersion;

        public FilterByApiVersionFilter(Func<SwaggerDocument, int> getApiVersion, Func<string, int> getPathApiVersion)
        {
            _getApiVersion = getApiVersion;
            _getPathApiVersion = getPathApiVersion;
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            var docVersion = _getApiVersion(swaggerDoc);
            foreach (var path in swaggerDoc.paths.ToList())
            {
                if (docVersion != _getPathApiVersion(path.Key))
                {
                    swaggerDoc.paths.Remove(path);
                }
            }

            var visitedDefinitions = new List<string>();
            var listOfSchemaWithReference = swaggerDoc.paths
                .SelectMany(x => x.Value.EnumerateOperations()) //Find operation by path
                .SelectMany(x => x.EnumerateSchema()) //Find schema by operation
                .SelectMany(x =>
                    x.EnumerateSchema(swaggerDoc.definitions, visitedDefinitions)) //Find Schema by schema (dependent schema)
                .Where(x => x?.@ref != null ||
                            x?.items?.@ref != null) //I only want the schema that reference a definition.
                .Select(x =>
                    ((x.@ref) ?? (x.items?.@ref))?.Replace("#/definitions/",
                        string.Empty)) //remove the path and keep the Model name
                .Distinct()
                .ToHashSet();

            //Not finding a definition in the built list of references means its unreferenced and can be removed.
            var listOfUnreferencedDefinition = swaggerDoc.definitions
                .Where(x => !listOfSchemaWithReference.Contains(x.Key))
                .ToList();

            foreach (var unreferencedDefinition in listOfUnreferencedDefinition)
            {
                swaggerDoc.definitions.Remove(unreferencedDefinition.Key);
            }
        }
    }
}