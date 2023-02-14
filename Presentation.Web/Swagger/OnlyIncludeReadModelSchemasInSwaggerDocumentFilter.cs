using System;
using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using Core.DomainModel;
using Swashbuckle.OData;

namespace Presentation.Web.Swagger
{
    public class OnlyIncludeReadModelSchemasInSwaggerDocumentFilter : IDocumentFilter
    {
        //Defines the real domain object references which are removed from EDM but still present in the docs unless removed like this
        private readonly ISet<string> _excludedProperties =
            new[]
            {
                nameof(IOwnedByOrganization.Organization),
                nameof(IReadModel<IHasId>.SourceEntity)
            }.ToHashSet(StringComparer.OrdinalIgnoreCase);

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            if (MatchODataApiExplorer(apiExplorer))
            {
                foreach (var definition in swaggerDoc.definitions)
                {
                    if (IsEntityType(definition))
                    {
                        OnlyIncludeReadModelDefinitions(definition);
                    }
                }
            }
        }

        private static bool MatchODataApiExplorer(IApiExplorer apiExplorer)
        {
            return apiExplorer.GetType().Assembly.FullName == typeof(ODataSwaggerProvider).Assembly.FullName;
        }

        private void OnlyIncludeReadModelDefinitions(KeyValuePair<string, Schema> definition)
        {
            var schema = definition.Value;
            var properties = schema.properties.ToList();
            if (IsReadModel(definition))
            {
                //Read models include lifecycle related properties which should not be in the docs since they are not in the edm model.
                var toRemove = properties.Where(x => _excludedProperties.Contains(x.Key)).ToList();
                toRemove.ForEach(property => properties.Remove(property));
            }
            else
            {
                //If not a read model controller, purge all properties
                foreach (var property in schema.properties)
                {
                    RemoveCollectionTypeProperty(property, properties);
                    RemoveReferenceTypeProperty(property, properties);
                }

            }
            schema.properties = properties.ToDictionary(property => property.Key, property => property.Value);
        }

        private static bool IsReadModel(KeyValuePair<string, Schema> definition)
        {
            return definition.Key.ToLowerInvariant().Contains("readmodel");
        }

        private static bool IsEntityType(KeyValuePair<string, Schema> definition)
        {
            return !definition.Key.Contains("ODataResponse[");
        }

        private static void RemoveCollectionTypeProperty(KeyValuePair<string, Schema> property, ICollection<KeyValuePair<string, Schema>> properties)
        {
            if (property.Value.type == "array" && property.Value.items.@ref != null)
            {
                properties.Remove(property);
            }
        }

        private static void RemoveReferenceTypeProperty(KeyValuePair<string, Schema> property, ICollection<KeyValuePair<string, Schema>> properties)
        {
            if (property.Value.type == null && property.Value.@ref != null)
            {
                properties.Remove(property);
            }
        }
    }
}