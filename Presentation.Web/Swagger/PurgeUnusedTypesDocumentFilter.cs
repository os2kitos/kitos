using System;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Presentation.Web.Swagger
{
    public class PurgeUnusedTypesDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            var listOfSchemaWithReference = swaggerDoc.paths
                .SelectMany(x => x.Value.EnumerateOperations()) //Find operation by path
                .SelectMany(x => x.EnumerateSchema()) //Find schema by operation
                .SelectMany(x =>
                    x.StartSchemaEnumeration(swaggerDoc.definitions)) //Find Schema by schema (dependent schema)
                .Where(x => x?.@ref != null ||
                            x?.items?.@ref != null) //I only want the schema that reference a definition.
                .Select(x =>
                    (x.@ref ?? x.items?.@ref)?.Replace("#/definitions/",
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
